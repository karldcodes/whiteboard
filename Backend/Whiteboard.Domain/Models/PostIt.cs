namespace Whiteboard.Domain.Models;

public class PostIt{
    public Guid Id {get; set;}
    public int X {get; set; } 
    public int Y {get; set;} 
    public int W {get; set;}
    public int H {get; set;} 
    public string Color {get; set;} = "";
    public string Label {get; set;} = "";
    public long? Version {get; set;}
}