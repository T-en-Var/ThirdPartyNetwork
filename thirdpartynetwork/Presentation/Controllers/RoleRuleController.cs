using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using thirdpartynetwork.Core.Models;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/networks/{networkId:guid}/roles/{roleId:guid}/rules")]
[ApiController]
public class RoleRuleController(IRoleRuleService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RoleRuleDto>))]
    public IActionResult GetRoleRules(Guid networkId, Guid roleId)
    {
        var roleRules = service.GetByRoleId(networkId, roleId);
        return Ok(mapper.Map<IEnumerable<RoleRuleDto>>(roleRules));
    }

    [HttpGet("{ruleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleRuleDto))]
    [ProducesResponseType(404)]
    public IActionResult GetRoleRule(Guid networkId, Guid roleId, Guid ruleId)
    {
        var roleRule = service.GetByIds(networkId, roleId, ruleId);
        return roleRule != null ? Ok(mapper.Map<RoleRuleDto>(roleRule)) : NotFound();
    }

    [HttpPost("{ruleId:guid}")]
    [ProducesResponseType(201, Type = typeof(RoleRuleDto))]
    [ProducesResponseType(404)]
    public IActionResult CreateRoleRule(Guid networkId, Guid roleId, Guid ruleId,
        CreateRoleRuleDto createRoleRule)
    {
        var roleRule = service.Create(networkId, roleId, ruleId, createRoleRule);
        return Ok(mapper.Map<RoleRuleDto>(roleRule));
    }

    [HttpPut("{ruleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RoleRuleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRoleRule(Guid networkId, Guid roleId, Guid ruleId, UpdateRoleRuleDto updateRoleRule)
    {
        var roleRule = service.Update(networkId, roleId, ruleId, updateRoleRule);
        return Ok(mapper.Map<RoleRuleDto>(roleRule));
    }

    [HttpDelete("{ruleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteRoleRule(Guid networkId, Guid roleId, Guid ruleId)
    {
        service.DeleteByIds(networkId, roleId, ruleId);
        return NoContent();
    }
}