using System;

namespace XLauncher.Entities.Environments
{

  using Common;

  public partial class Box : IValidable
  {

    public void Validate() {

      if (String.IsNullOrWhiteSpace(Text))
        throw new ValidationException("Box attribute 'text' must be non-empty.");
      Text = Text.Trim();

      if (Controls == null || Controls.Length == 0)
        throw new ValidationException($"Invalid Box '{Text}'. At least one control is required.");

      try {
        foreach (var c in Controls)
          c.Validate();
      }
      catch (ValidationException vex) {
        vex.Parents.Add($"Box => {Text}");
        throw;
      }

    }

  }
}
