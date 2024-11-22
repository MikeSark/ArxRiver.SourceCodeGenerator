using ArxRiver.SourceGenerator.XUnitTest.TestModels;
using FluentAssertions;
using TaskManager = ArxRiver.SourceGenerator.XUnitTest.TestModels.TaskManager.TaskManager;

namespace ArxRiver.SourceGenerator.XUnitTest;

public class TaskManagerUnitTest
{
    // write a xunit method for Animal class
    [Fact]
    public async Task task_manager_no_result_task_manager_test()
    {
        var noReturnString = string.Empty;
        var taskManager = new TaskManager();

        taskManager.EnqueueTask(async () => { await Task.Delay(1000); }, (exception) => noReturnString = exception?.Message);

        await taskManager.WaitAllTasksAsync();

        noReturnString.Should().BeNullOrEmpty("Exception was thrown for no return task");
    }


    [Fact]
    public async Task task_manager_with_request_response_task_execution_test()
    {
        var taskManager = new TaskManager();

        taskManager.EnqueueTask<TaskRequest, TaskResponse>(
            new TaskRequest { Id = 10, Result = "From Request" },
            async (request) =>
            {
                
                await Task.Delay(1000);
                return new TaskResponse { Id = request.Id, ResponseMessage = request.Result }; // Simulate processing request
            },
            (response, exception) =>
            {
                if (exception != null)
                {
                    Console.WriteLine("Error: " + exception.Message);
                }
                else
                {
                    Console.WriteLine("Response: " + response.ResponseMessage);
                }
            });

        await taskManager.WaitAllTasksAsync();
    }


    [Fact]
    public async Task task_manager_with_result_task_manager_test()
    {
        var taskManager = new TaskManager();
        var resultContent = "";
        var exceptionMessage = "";
        var queueCount = 0;
        var tasksCount = 0;

        taskManager.TasksRemainingChanged += (i, i1) =>
        {
            queueCount = i;
            tasksCount = i1;
        };

        taskManager.EnqueueTask<string>(async () =>
        {
            await Task.Delay(2000);
            return "Hello";
        }, (result, exception) =>
        {
            resultContent = result;
            exceptionMessage = exception?.Message;
        });

        tasksCount.Should().Be(1, "One task is still pending");

        await taskManager.WaitAllTasksAsync();

        resultContent.Should().Be("Hello", "Result should have been Hello");
        exceptionMessage.Should().BeNullOrEmpty("Exception should be null");
    }

    [Fact]
    public async Task task_manager_with_2_tasks_manager_test()
    {
        var taskManager = new TaskManager();
        var resultContent = "";
        var exceptionMessage = "";
        var queueCount = 0;
        var tasksCount = 0;

        taskManager.TasksRemainingChanged += (i, i1) =>
        {
            queueCount = i;
            tasksCount = i1;
        };

        taskManager.EnqueueTask<string>(async () =>
        {
            await Task.Delay(10000);
            return "Hello";
        }, (result, exception) =>
        {
            resultContent = result;
            exceptionMessage = exception?.Message;
        });

        taskManager.EnqueueTask(async () =>
        {
            await Task.Delay(10000);
            return 1;
        }, (result, exception) => Console.WriteLine($"Result: {result} - Exception: {exception?.Message}"));

        tasksCount.Should().Be(2, "One task is still pending");

        await taskManager.WaitAllTasksAsync();

        resultContent.Should().Be("Hello", "Result should have been Hello");
        exceptionMessage.Should().BeNullOrEmpty("Exception should be null");
    }
}