using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Turnos.C.Data;
using Turnos.C.Helpers;
using Turnos.C.Models;
using Turnos.C.Models.ViewModels;

namespace Turnos.C.Controllers
{
    public class AccountController : Controller
    {
        private readonly TurnosContext _context;
        private readonly UserManager<Persona> _userManager;
        private readonly SignInManager<Persona> _signInManager;
        private readonly RoleManager<Rol> _roleManager;

        //Registracion de usuario

        public AccountController(
            TurnosContext context,
            UserManager<Persona> userManager, 
            SignInManager<Persona> signInManager,
            RoleManager<Rol> roleManager
            )
        {
            this._context = context;
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        //public IActionResult Registrar()
        //{
        //    Console.WriteLine("DEBUG: Acción GET /Account/Registrar ejecutada");
        //    return View();
        //}

        public IActionResult Registrar()
        {
            if (User.Identity.IsAuthenticated)
            {
                TempData["Mensaje"] = "Usted ya se encuentra con sesión iniciada, para registrarse como usuario primero debe cerrar sesión.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Registrar(
            [Bind("Email,Password,ConfirmacionPassword")]
            RegistroUsuario viewModel)
        {
            if (ModelState.IsValid)
            {
                var pacienteACrear = new Paciente
                {
                    Email = viewModel.Email,
                    UserName = viewModel.Email,
                };

                var result = await _userManager.CreateAsync(pacienteACrear, viewModel.Password);

                if (result.Succeeded)
                {
                    var resAddRole = await _userManager.AddToRoleAsync(pacienteACrear, Config.PacienteRolName);
                    if (resAddRole.Succeeded)
                    {
                        await _signInManager.SignInAsync(pacienteACrear, isPersistent: false);

                        // Redirige al mismo Edit unificado que usa Admin, pero el paciente solo puede editarse a sí mismo
                        return RedirectToAction("Edit", "Pacientes", new { id = pacienteACrear.Id });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"No se pudo agregar rol {Config.PacienteRolName}");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult IniciarSesion(string returnUrl)
        {

            //recibimos el url por parametro 
            TempData["ReturnUrl"] = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                TempData["Mensaje"] = "Usted ya se encuentra con sesión iniciada, para iniciar con otro usuario primero debe cerrar sesión.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(Login model)
        {
            string returnUrl = TempData["ReturnUrl"] as string;

            if (ModelState.IsValid)
            {

               var result = await  _signInManager.PasswordSignInAsync(
                                userName: model.Email,
                                password: model.Password,
                                isPersistent: model.Recordarme,
                                lockoutOnFailure: false
                                );
                if (result.Succeeded)
                {
                    //si el string del url no es nulo, redireccionamos a dnd se queria acceder
                    //en un principio pero luego de iniciar sesion
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction(
                        actionName:"Index",
                        controllerName: "Home"
                        );
                }
                ModelState.AddModelError(
                    key:String.Empty, 
                    errorMessage: "Inicio de sesion invalido");
            }

            return View(model);
        }

        public async Task<IActionResult> CerrarSesion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(
                actionName: "Index",
                controllerName: "Home"
                );
        }

        [Authorize(Roles = Config.AdminRolName)]
        public async Task <IActionResult> ListarRoles()
        {
            var roles = _context.Roles.ToList();

            return View(roles);
        }

        public IActionResult AccesoDenegado(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

    }
}
