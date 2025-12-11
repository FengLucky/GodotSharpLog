namespace LF;
public interface IUIListItem<TData>
{
    TData Data { get; set; }
    void Refresh();
}