namespace LF;

public interface IPoolable
{
    int Version { get; set; }
    void OnGet() {}
    void OnRelease(){}
    void OnDestroy(){}
}