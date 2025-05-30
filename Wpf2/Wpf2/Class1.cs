using System;
using System.ComponentModel;
using System.Windows;

public class MessageModel : INotifyPropertyChanged
{
    private DateTime timestamp;

    public string Username { get; set; }
    public string Message { get; set; }

    public DateTime Timestamp
    {
        get => timestamp;
        set
        {
            timestamp = value;
            OnPropertyChanged(nameof(Date));
            OnPropertyChanged(nameof(ExactTimestamp));
        }
    }

    public HorizontalAlignment Alignment { get; set; }

    public string Date
    {
        get
        {
            var now = DateTime.Now;
            var diff = now - Timestamp;

            if (diff.TotalMinutes < 1)
                return "Now";
            if (diff.TotalMinutes < 15)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return Timestamp.ToString("HH:mm");
            return Timestamp.ToString("dd/MM/yyyy");
        }
    }

    public string ExactTimestamp => Timestamp.ToString("dd MMM yyyy HH:mm:ss");

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
