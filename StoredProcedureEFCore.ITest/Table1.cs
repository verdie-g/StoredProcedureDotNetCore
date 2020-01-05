using System;

namespace StoredProcedureEFCore.ITest
{
  public partial class Table1
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public bool Active { get; set; }
    // name_with_underscore in db
    public int NameWithUnderscore { get; set; }
  }
}
