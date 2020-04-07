using System;
public class TodoItem
{
    public Guid Id { get; private set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public bool IsComplete { get; private set; }
    public DateTime OnCreate{get; private set;} 
    public TodoItem(){
        Id = new Guid();
        OnCreate = DateTime.Now;
        IsComplete = false;
    }
    
}