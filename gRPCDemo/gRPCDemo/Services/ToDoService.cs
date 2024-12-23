using Grpc.Core;
using gRPCDemo.Data;
using gRPCDemo.Models;
using GrpcDemo.Protos;
using Microsoft.EntityFrameworkCore;

namespace gRPCDemo.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _context;
    public ToDoService(AppDbContext context)
    {
        _context = context;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if(request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _context.ToDoItems.AddAsync(toDoItem);
        await _context.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse{
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resource index must be greater than 0"));
        
        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(toDoItem != null)
        {
            return await Task.FromResult(new ReadToDoResponse{
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"No task with id-{request.Id}"));
    }

    public override async Task<GetAllResponse> ListToDos(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var toDoItem = await _context.ToDoItems.ToListAsync();

        foreach(var toDo in toDoItem)
        {
            response.ToDo.Add(new ReadToDoResponse{
                Id = toDo.Id,
                Title = toDo.Title,
                Description = toDo.Description,
                ToDoStatus = toDo.ToDoStatus
            });
        }
        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id-{request.Id}"));
        
        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.ToDoStatus = request.ToDoStatus;

        await _context.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse{
            Id = toDoItem.Id
        });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
         if(request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

        if(toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No task with id-{request.Id}"));

        _context.Remove(toDoItem);

        await _context.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse{
            Id = toDoItem.Id
        });
    }
}