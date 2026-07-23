{Include,README.md}

## Updates

| Version | Description |
|---------|-------------|
| 26.2117.4453 | Initial publication to NuGet. |
| 26.2224.4542 | The **GetSpecifiedOrWorking** function has been renamed to **GetWorkingDocument**, which contains a call to the **OpenWorkingDocument** method that executes if the document hasn't yet been loaded. |
| 26.2723.3653 | When **InputFilename** is not found for an action, its value is now retained; The **ActionItemBase.InputFileFound(TAction action)** static method has been added to check the **InputFilename** value and **InputNames** collection for indication of whether the physical file has been found at this level, if specified. False is only returned if the file was specified and not found. |

## More Information

For more information, please see the GitHub project:
[danielanywhere/ActionEngine](https://github.com/danielanywhere/ActionEngine)

Full API documentation is available at this library's [GitHub User Page](https://danielanywhere.github.io/ActionEngine).

