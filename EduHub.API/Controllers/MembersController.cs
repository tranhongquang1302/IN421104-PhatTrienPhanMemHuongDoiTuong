using EduHub.Application.Members;
using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

/// <summary>Quản lý thành viên câu lạc bộ.</summary>
[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly GetMembersQueryHandler _getMembersHandler;
    private readonly GetMemberByIdQueryHandler _getMemberByIdHandler;
    private readonly CreateMemberCommandHandler _createMemberHandler;
    private readonly DeleteMemberCommandHandler _deleteMemberHandler;

    public MembersController(
        GetMembersQueryHandler getMembersHandler,
        GetMemberByIdQueryHandler getMemberByIdHandler,
        CreateMemberCommandHandler createMemberHandler,
        DeleteMemberCommandHandler deleteMemberHandler)
    {
        _getMembersHandler = getMembersHandler;
        _getMemberByIdHandler = getMemberByIdHandler;
        _createMemberHandler = createMemberHandler;
        _deleteMemberHandler = deleteMemberHandler;
    }

    /// <summary>Lấy danh sách thành viên.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Member>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var members = await _getMembersHandler.HandleAsync(new GetMembersQuery(), ct);
        return Ok(members);
    }

    /// <summary>Lấy một thành viên theo Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Member), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var member = await _getMemberByIdHandler.HandleAsync(new GetMemberByIdQuery(id), ct);
        return member is null ? NotFound() : Ok(member);
    }

    /// <summary>Thêm thành viên mới.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Member), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request, CancellationToken ct)
    {
        try
        {
            var command = new CreateMemberCommand(request.StudentId, request.Name, request.Email);
            var member = await _createMemberHandler.HandleAsync(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Xoá một thành viên.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _deleteMemberHandler.HandleAsync(new DeleteMemberCommand(id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

/// <summary>Dữ liệu client gửi lên khi thêm thành viên.</summary>
public record CreateMemberRequest(string StudentId, string Name, string Email);
