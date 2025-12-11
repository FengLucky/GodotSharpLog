namespace LF;
public partial class UIPage<T>:UIPage
{
    public T Args => Holder.GetArgs<T>();
}