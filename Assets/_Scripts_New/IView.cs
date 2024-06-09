namespace TinyRogue
{
    public interface IView<T>
    {
        public void Initialize(T model);
    }
}