namespace Flow.Store.Contracts
{
    public interface IAction
    {
        string Identifier { get; set; }
        string Node { get; set; }
        object Data { get; set; }
    }
}
