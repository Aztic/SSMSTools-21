namespace SSMSTools_21.Factories.Interfaces
{
    public interface IWindowFactory
    {
        T CreateWindow<T>() where T : class;
    }
}