using System;
public class TodoItem
{
    public int id { get; private set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public long UserId {get; set;}
    public DateTime OnCreate{get; private set;} 
    public TodoItem(){
        OnCreate = DateTime.Now;
    }
    
}