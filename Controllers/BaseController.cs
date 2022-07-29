using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Softaway.Identidade.API.Controllers
{
    [ApiController]
    public abstract class BaseController : Controller
    {

        protected ICollection<string> Errors = new List<string>();

        //It will response every type of Result. From OK to BadRequest
        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                return Ok(result);
            }

            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]> 
            {
                {"Mensagens", Errors.ToArray() }
            }));
        }

        protected bool OperacaoValida()
        {
            return !Errors.Any();
        }

        //Follow, methods for add/remove erros to the Errors colection
        protected void AdicionarErroProcessamento(string erro)
        {
            Errors.Add(erro);
        }

        protected void LimparErrosProcessamento()
        {
            Errors.Clear();
        }

        //Method overload for using in the case of validation errors occured in the ViewModel
        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);

            foreach (var erro in erros)
            {
                AdicionarErroProcessamento(erro.ErrorMessage);
            }

            return CustomResponse();
        }
    }
}
