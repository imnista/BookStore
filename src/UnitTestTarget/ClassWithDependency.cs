namespace Bec
{
  public class ClassWithDependency
  {
    private readonly IDependency _worker;

    public ClassWithDependency(IDependency worker)
    {
      _worker = worker;
    }

    public string GetAliasName(string name)
    {
      return _worker.GetNickName(name);
    }
  }


  public interface IDependency
  {
    string GetNickName(string name);
  }
}
