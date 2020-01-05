using System;

namespace StoredProcedureEFCore.ITest
{
  internal class Model : ModelSlim
  {
    public DateTime Date { get; set; }
    public bool Active { get; set; }
    public TestEnum NameWithUnderscore { get; set; }
  }

  enum TestEnum
  {
    Yes = 0,
    No = 1
  }
}
