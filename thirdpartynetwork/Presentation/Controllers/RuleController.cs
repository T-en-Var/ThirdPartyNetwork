using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/rules")]
[ApiController]
public class RuleController(IRuleService service, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<RuleDto>))]
    public IActionResult GetRules()
    {
        var rules = service.GetAll();
        return Ok(mapper.Map<IEnumerable<RuleDto>>(rules));
    }

    [HttpGet("{ruleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RuleDto))]
    public IActionResult GetRule(Guid ruleId)
    {
        var rule = service.GetById(ruleId);
        return rule != null ? Ok(mapper.Map<RuleDto>(rule)) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(201, Type = typeof(RuleDto))]
    public IActionResult CreateRule([FromBody] CreateRuleDto createRule)
    {
        var rule = service.Create(createRule);
        return Ok(mapper.Map<RuleDto>(rule));
    }

    [HttpPut("{ruleId:guid}")]
    [ProducesResponseType(200, Type = typeof(RuleDto))]
    [ProducesResponseType(404)]
    public IActionResult UpdateRule(Guid ruleId, [FromBody] UpdateRuleDto updateRule)
    {
        var rule = service.Update(ruleId, updateRule);
        return Ok(mapper.Map<RuleDto>(rule));
    }

    [HttpDelete("{ruleId:guid}")]
    [ProducesResponseType(204)]
    public IActionResult DeleteRule(Guid ruleId)
    {
        service.DeleteById(ruleId);
        return NoContent();
    }
}
