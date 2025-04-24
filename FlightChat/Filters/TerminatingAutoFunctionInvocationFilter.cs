using Microsoft.SemanticKernel;

namespace FlightChat;

public class TerminatingAutoFunctionInvocationFilter : IAutoFunctionInvocationFilter
{
    private readonly HashSet<string> _functionNames;

    public TerminatingAutoFunctionInvocationFilter(params string[] functionNames)
    {
        _functionNames = new HashSet<string>(functionNames);
    }

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(context);

        if (_functionNames.Contains(context.Function.Name))
        {
            context.Terminate = true;
        }
    }
}