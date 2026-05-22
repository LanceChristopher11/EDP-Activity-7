using MySqlConnector;

namespace ConcertEventSystemUI;

public class DatabaseConnection
{
    private const string Server   = "localhost";
    private const string Port     = "3306";
    private const string Database = "concert_event_db";
    private const string User     = "root";
    private const string Pass     = "";

    public static readonly string ConnectionString =
        $"Server={Server};Port={Port};Database={Database};" +
        $"User ID={User};Password={Pass};CharSet=utf8mb4;";

    public static MySqlConnection GetConnection()
    {
        var conn = new MySqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }


    public static UserRecord? ValidateLogin(string username, string plainPassword)
    {
        const string sql = """
            SELECT id, username, email, full_name, role, is_active
            FROM   users
            WHERE  username = @u AND password = SHA2(@p, 256) AND is_active = 1
            LIMIT  1
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", username.Trim());
        cmd.Parameters.AddWithValue("@p", plainPassword);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new UserRecord(r.GetInt32("id"), r.GetString("username"),
            r.GetString("email"), r.GetString("full_name"), r.GetString("role"));
    }

    public static UserRecord? FindAccountByEmail(string email)
    {
        const string sql = """
            SELECT id, username, email, full_name, role, is_active
            FROM users WHERE email = @e LIMIT 1
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@e", email.Trim());
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new UserRecord(r.GetInt32("id"), r.GetString("username"),
            r.GetString("email"), r.GetString("full_name"), r.GetString("role"),
            r.GetBoolean("is_active"));
    }

    public static bool ResetPassword(int userId, string newPlainPassword)
    {
        const string sql = "UPDATE users SET password = SHA2(@p, 256), updated_at = NOW() WHERE id = @id";
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@p", newPlainPassword);
        cmd.Parameters.AddWithValue("@id", userId);
        return cmd.ExecuteNonQuery() > 0;
    }


    public static List<UserRecord> GetAllUsers(string search = "", bool? statusFilter = null)
    {
        var conds = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            conds.Add("(username LIKE @s OR full_name LIKE @s OR email LIKE @s)");
        if (statusFilter.HasValue) conds.Add("is_active = @a");
        string where = conds.Count > 0 ? "WHERE " + string.Join(" AND ", conds) : "";

        var list = new List<UserRecord>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(
            $"SELECT id,username,email,full_name,role,is_active FROM users {where} ORDER BY id", conn);
        if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@s", $"%{search.Trim()}%");
        if (statusFilter.HasValue) cmd.Parameters.AddWithValue("@a", statusFilter.Value ? 1 : 0);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new UserRecord(r.GetInt32("id"), r.GetString("username"),
                r.GetString("email"), r.GetString("full_name"), r.GetString("role"),
                r.GetBoolean("is_active")));
        return list;
    }

    public static bool AddUser(string username, string email, string fullName,
                               string role, string plainPassword)
    {
        const string sql = """
            INSERT INTO users (username,email,password,full_name,role,is_active)
            VALUES (@u,@e,SHA2(@p,256),@f,@r,1)
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@u", username.Trim());
        cmd.Parameters.AddWithValue("@e", email.Trim());
        cmd.Parameters.AddWithValue("@p", plainPassword);
        cmd.Parameters.AddWithValue("@f", fullName.Trim());
        cmd.Parameters.AddWithValue("@r", role);
        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool UpdateUser(int id, string email, string fullName,
                                  string role, string newPlainPassword = "")
    {
        string sql = string.IsNullOrWhiteSpace(newPlainPassword)
            ? "UPDATE users SET email=@e,full_name=@f,role=@r,updated_at=NOW() WHERE id=@id"
            : "UPDATE users SET email=@e,full_name=@f,role=@r,password=SHA2(@p,256),updated_at=NOW() WHERE id=@id";
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@e", email.Trim());
        cmd.Parameters.AddWithValue("@f", fullName.Trim());
        cmd.Parameters.AddWithValue("@r", role);
        cmd.Parameters.AddWithValue("@id", id);
        if (!string.IsNullOrWhiteSpace(newPlainPassword))
            cmd.Parameters.AddWithValue("@p", newPlainPassword);
        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool SetUserActive(int id, bool active)
    {
        const string sql = "UPDATE users SET is_active=@a,updated_at=NOW() WHERE id=@id";
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@a", active ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }


    public static int GetTotalEvents()
    { using var c = GetConnection(); using var cmd = new MySqlCommand("SELECT COUNT(*) FROM events", c); return Convert.ToInt32(cmd.ExecuteScalar()); }

    public static int GetTotalVenues()
    { using var c = GetConnection(); using var cmd = new MySqlCommand("SELECT COUNT(*) FROM venues", c); return Convert.ToInt32(cmd.ExecuteScalar()); }

    public static int GetTotalArtists()
    { using var c = GetConnection(); using var cmd = new MySqlCommand("SELECT COUNT(*) FROM artists", c); return Convert.ToInt32(cmd.ExecuteScalar()); }

    public static decimal GetTotalRevenue()
    {
        using var c = GetConnection();
        using var cmd = new MySqlCommand("SELECT COALESCE(SUM(ticket_price*tickets_sold),0) FROM ticket_sales", c);
        return Convert.ToDecimal(cmd.ExecuteScalar());
    }

    public static UpcomingEventInfo? GetNextUpcomingEvent()
    {
        const string sql = """
            SELECT e.event_name,e.event_date,v.venue_name,v.location
            FROM events e JOIN venues v ON e.venue_id=v.venue_id
            WHERE e.event_date>=CURDATE() ORDER BY e.event_date ASC LIMIT 1
            """;
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new UpcomingEventInfo(r.GetString("event_name"), r.GetDateTime("event_date"),
            r.GetString("venue_name"), r.GetString("location"));
    }

    public static List<RecentSaleInfo> GetRecentSales(int limit = 5)
    {
        var list = new List<RecentSaleInfo>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand($"""
            SELECT e.event_name,a.artist_name,ts.tickets_sold,ts.ticket_price
            FROM ticket_sales ts
            JOIN events e ON ts.event_id=e.event_id
            JOIN artists a ON ts.artist_id=a.artist_id
            ORDER BY ts.ticket_id DESC LIMIT {limit}
            """, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new RecentSaleInfo(r.GetString("event_name"), r.GetString("artist_name"),
                r.GetInt32("tickets_sold"), r.GetDecimal("ticket_price")));
        return list;
    }


    public static List<EventReportRow> GetEventReport(int venueId = 0)
    {
        string sql = """
            SELECT e.event_name,e.event_date,v.venue_name,v.location,v.capacity,o.organizer_name
            FROM events e
            JOIN venues v ON e.venue_id=v.venue_id
            JOIN organizers o ON e.organizer_id=o.organizer_id
            """ + (venueId > 0 ? " WHERE e.venue_id=@vid" : "") + " ORDER BY e.event_date ASC";
        var list = new List<EventReportRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        if (venueId > 0) cmd.Parameters.AddWithValue("@vid", venueId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new EventReportRow(r.GetString("event_name"), r.GetDateTime("event_date"),
                r.GetString("venue_name"), r.GetString("location"), r.GetInt32("capacity"), r.GetString("organizer_name")));
        return list;
    }

    public static List<SalesReportRow> GetSalesReport(int venueId = 0)
    {
        string sql = """
            SELECT e.event_name,e.event_date,v.venue_name,a.artist_name,
                   ts.ticket_price,ts.tickets_sold,(ts.ticket_price*ts.tickets_sold) AS revenue
            FROM ticket_sales ts
            JOIN events e ON ts.event_id=e.event_id
            JOIN venues v ON e.venue_id=v.venue_id
            JOIN artists a ON ts.artist_id=a.artist_id
            """ + (venueId > 0 ? " WHERE e.venue_id=@vid" : "") + " ORDER BY revenue DESC";
        var list = new List<SalesReportRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        if (venueId > 0) cmd.Parameters.AddWithValue("@vid", venueId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new SalesReportRow(r.GetString("event_name"), r.GetDateTime("event_date"),
                r.GetString("venue_name"), r.GetString("artist_name"), r.GetDecimal("ticket_price"),
                r.GetInt32("tickets_sold"), r.GetDecimal("revenue")));
        return list;
    }

    public static List<ArtistReportRow> GetArtistReport(int artistId = 0)
    {
        string sql = """
            SELECT a.artist_name,a.genre,e.event_name,e.event_date,v.venue_name,v.location
            FROM ticket_sales ts
            JOIN artists a ON ts.artist_id=a.artist_id
            JOIN events e ON ts.event_id=e.event_id
            JOIN venues v ON e.venue_id=v.venue_id
            """ + (artistId > 0 ? " WHERE a.artist_id=@aid" : "") + " ORDER BY e.event_date ASC";
        var list = new List<ArtistReportRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        if (artistId > 0) cmd.Parameters.AddWithValue("@aid", artistId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new ArtistReportRow(r.GetString("artist_name"), r.GetString("genre"),
                r.GetString("event_name"), r.GetDateTime("event_date"),
                r.GetString("venue_name"), r.GetString("location")));
        return list;
    }

    public static List<VenueReportRow> GetVenueReport()
    {
        const string sql = """
            SELECT v.venue_name,v.location,v.capacity,
                   COUNT(DISTINCT e.event_id) AS event_count,
                   COALESCE(SUM(ts.tickets_sold),0) AS total_sold
            FROM venues v
            LEFT JOIN events e ON v.venue_id=e.venue_id
            LEFT JOIN ticket_sales ts ON e.event_id=ts.event_id
            GROUP BY v.venue_id,v.venue_name,v.location,v.capacity
            ORDER BY total_sold DESC
            """;
        var list = new List<VenueReportRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new VenueReportRow(r.GetString("venue_name"), r.GetString("location"),
                r.GetInt32("capacity"), r.GetInt32("event_count"), r.GetInt32("total_sold")));
        return list;
    }

    public static List<(int Id, string Name)> GetVenueList()
    {
        var list = new List<(int, string)>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand("SELECT venue_id,venue_name FROM venues ORDER BY venue_name", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add((r.GetInt32("venue_id"), r.GetString("venue_name")));
        return list;
    }

    public static List<(int Id, string Name)> GetArtistList()
    {
        var list = new List<(int, string)>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand("SELECT artist_id,artist_name FROM artists ORDER BY artist_name", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add((r.GetInt32("artist_id"), r.GetString("artist_name")));
        return list;
    }

    public static List<(int Id, string Name)> GetEventList()
    {
        var list = new List<(int, string)>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand("SELECT event_id,event_name FROM events ORDER BY event_date", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add((r.GetInt32("event_id"), r.GetString("event_name")));
        return list;
    }

    public static List<(int Id, string Name)> GetOrganizerList()
    {
        var list = new List<(int, string)>();
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand("SELECT organizer_id,organizer_name FROM organizers ORDER BY organizer_name", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add((r.GetInt32("organizer_id"), r.GetString("organizer_name")));
        return list;
    }

    /// Summary is dis inserts a new ticket sale. Returns the new ticket_id.
    public static int AddTicketSale(int eventId, int artistId, decimal ticketPrice, int ticketsSold)
    {
        const string sql = """
            INSERT INTO ticket_sales (event_id, artist_id, ticket_price, tickets_sold)
            VALUES (@eid, @aid, @price, @sold);
            SELECT LAST_INSERT_ID();
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@eid",   eventId);
        cmd.Parameters.AddWithValue("@aid",   artistId);
        cmd.Parameters.AddWithValue("@price", ticketPrice);
        cmd.Parameters.AddWithValue("@sold",  ticketsSold);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<TicketSaleRow> GetTicketSales()
    {
        const string sql = """
            SELECT ts.ticket_id, e.event_name, e.event_date, a.artist_name,
                   ts.ticket_price, ts.tickets_sold,
                   (ts.ticket_price * ts.tickets_sold) AS revenue
            FROM   ticket_sales ts
            JOIN   events  e ON ts.event_id  = e.event_id
            JOIN   artists a ON ts.artist_id = a.artist_id
            ORDER  BY ts.ticket_id DESC
            """;
        var list = new List<TicketSaleRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new TicketSaleRow(
                r.GetInt32("ticket_id"), r.GetString("event_name"),
                r.GetDateTime("event_date"), r.GetString("artist_name"),
                r.GetDecimal("ticket_price"), r.GetInt32("tickets_sold"),
                r.GetDecimal("revenue")));
        return list;
    }


    /// Summary is dis books a new event. Returns the new event_id.
    public static int AddEvent(string eventName, DateTime eventDate,
                               int venueId, int organizerId, string createdBy)
    {
        const string sql = """
            INSERT INTO events (event_name, event_date, venue_id, organizer_id, status, created_by)
            VALUES (@name, @date, @vid, @oid, 'Scheduled', @by);
            SELECT LAST_INSERT_ID();
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", eventName.Trim());
        cmd.Parameters.AddWithValue("@date", eventDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@vid",  venueId);
        cmd.Parameters.AddWithValue("@oid",  organizerId);
        cmd.Parameters.AddWithValue("@by",   createdBy);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<EventBookingRow> GetEventBookings()
    {
        const string sql = """
            SELECT e.event_id, e.event_name, e.event_date,
                   v.venue_name, v.location,
                   o.organizer_name,
                   COALESCE(e.status,'Scheduled') AS status,
                   COALESCE(e.created_by,'—')     AS created_by
            FROM   events e
            JOIN   venues     v ON e.venue_id     = v.venue_id
            JOIN   organizers o ON e.organizer_id = o.organizer_id
            ORDER  BY e.event_date DESC
            """;
        var list = new List<EventBookingRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new EventBookingRow(
                r.GetInt32("event_id"), r.GetString("event_name"),
                r.GetDateTime("event_date"), r.GetString("venue_name"),
                r.GetString("location"), r.GetString("organizer_name"),
                r.GetString("status"), r.GetString("created_by")));
        return list;
    }

    /// Summary is dis books an artist for an event. Returns the new booking_id.
    public static int AddArtistBooking(int eventId, int artistId, decimal fee,
                                       string status, string notes, string bookedBy)
    {
        const string sql = """
            INSERT INTO artist_bookings (event_id, artist_id, booking_fee, status, notes, booked_by)
            VALUES (@eid, @aid, @fee, @status, @notes, @by);
            SELECT LAST_INSERT_ID();
            """;
        using var conn = GetConnection();
        using var cmd  = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@eid",    eventId);
        cmd.Parameters.AddWithValue("@aid",    artistId);
        cmd.Parameters.AddWithValue("@fee",    fee);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@notes",  notes);
        cmd.Parameters.AddWithValue("@by",     bookedBy);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<ArtistBookingRow> GetArtistBookings()
    {
        const string sql = """
            SELECT ab.booking_id, a.artist_name, a.genre,
                   e.event_name, e.event_date,
                   v.venue_name,
                   ab.booking_fee, ab.status,
                   COALESCE(ab.notes,'')     AS notes,
                   COALESCE(ab.booked_by,'—') AS booked_by,
                   ab.booked_at
            FROM   artist_bookings ab
            JOIN   artists a ON ab.artist_id = a.artist_id
            JOIN   events  e ON ab.event_id  = e.event_id
            JOIN   venues  v ON e.venue_id   = v.venue_id
            ORDER  BY ab.booked_at DESC
            """;
        var list = new List<ArtistBookingRow>();
        using var conn = GetConnection(); using var cmd = new MySqlCommand(sql, conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new ArtistBookingRow(
                r.GetInt32("booking_id"), r.GetString("artist_name"), r.GetString("genre"),
                r.GetString("event_name"), r.GetDateTime("event_date"), r.GetString("venue_name"),
                r.GetDecimal("booking_fee"), r.GetString("status"), r.GetString("notes"),
                r.GetString("booked_by"), r.GetDateTime("booked_at")));
        return list;
    }
}

public record UserRecord(int Id, string Username, string Email,
    string FullName, string Role, bool IsActive = true);

public record UpcomingEventInfo(string EventName, DateTime EventDate,
    string VenueName, string Location);

public record RecentSaleInfo(string EventName, string ArtistName,
    int TicketsSold, decimal TicketPrice);

public record EventReportRow(string EventName, DateTime EventDate, string VenueName,
    string Location, int Capacity, string OrganizerName);
public record SalesReportRow(string EventName, DateTime EventDate, string VenueName,
    string ArtistName, decimal TicketPrice, int TicketsSold, decimal Revenue);
public record ArtistReportRow(string ArtistName, string Genre, string EventName,
    DateTime EventDate, string VenueName, string Location);
public record VenueReportRow(string VenueName, string Location, int Capacity,
    int EventCount, int TotalSold);

public record TicketSaleRow(int TicketId, string EventName, DateTime EventDate,
    string ArtistName, decimal TicketPrice, int TicketsSold, decimal Revenue);
public record EventBookingRow(int EventId, string EventName, DateTime EventDate,
    string VenueName, string Location, string OrganizerName, string Status, string CreatedBy);
public record ArtistBookingRow(int BookingId, string ArtistName, string Genre,
    string EventName, DateTime EventDate, string VenueName, decimal BookingFee,
    string Status, string Notes, string BookedBy, DateTime BookedAt);