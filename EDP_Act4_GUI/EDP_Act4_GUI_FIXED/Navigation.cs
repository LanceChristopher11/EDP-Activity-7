namespace ConcertEventSystemUI;

internal static class Navigation
{
    public static void Open(Form current, Form next)
    {
        next.FormClosed += (_, _) =>
        {
            if (!current.IsDisposed)
            {
                current.Close();
            }
        };

        next.Show();
        current.Hide();
    }
}