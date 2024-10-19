using Infrastructure;

namespace ConsoleUI;

public interface IResultsSaver
{
    Task<Result<bool>> SaveUserIdsAsync(IEnumerable<ulong> users);
}

public class ResultsSaver: IResultsSaver
{
    public Task<Result<bool>> SaveUserIdsAsync(IEnumerable<ulong> users)
    {
        var usersToDisplay = 10;
        var numberOfUsers = users.Count();
        var local = users.Take(usersToDisplay).ToArray();
        Console.WriteLine($"There are {numberOfUsers} loyal users");
        if (numberOfUsers > usersToDisplay)
            Console.WriteLine($"The first {usersToDisplay} users are:");

        foreach (ulong userId in local)
        {
            Console.WriteLine(userId);
        }

        Console.WriteLine("<--------------->");
        return Task.FromResult(Result<bool>.Ok(true));
    }
}