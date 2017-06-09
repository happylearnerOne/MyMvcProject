using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using MyMvcProject.Models.Context;
using MyMvcProject.Models.Table;
using Npgsql;
using System.Data;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using MyMvcProject.Models.Converter;

namespace MyMvcProject.Controllers
{
    public class HomeController : Controller
    {
        protected SampleDbContext dbContext;
        protected DAContext daContext = new DAContext();
        public HomeController()
        {
            dbContext = new SampleDbContext();
        }
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult GetUser()
        {
            string sql = "select * from sys_user";
            //NpgsqlCommand cmd = new NpgsqlCommand(sql);
            //cmd.CommandText = sql.ToString();
            DataTable dt = daContext.QueryDataTable(sql);
            IEnumerable<sys_user> sys_user_list = Converter<sys_user>.ConvertToList(dt);
            return View(sys_user_list);
        }
        public ActionResult EditUser(int id)
        {
            string sql = "select * from sys_user where id = " + id;
            //NpgsqlCommand cmd = new NpgsqlCommand(sql);
            //cmd.CommandText = sql.ToString();
            DataTable dt = daContext.QueryDataTable(sql);
            sys_user sys_user = Converter<sys_user>.ConvertToObject(dt);
            return View(sys_user);
        }
        [HttpPost]
        public ActionResult EditUser(sys_user form)
        {
            string sql = "update sys_user set phone = '" + form.phone + "' where id = " + form.id;
            //NpgsqlCommand cmd = new NpgsqlCommand(sql);
            //cmd.CommandText = sql.ToString();
            daContext.TransactionBegin();
            daContext.ExecuteNonQuery(sql);
            daContext.TransactionEnd();
            return RedirectToAction("GetUser");
        }
        public ActionResult GetSysUser()
        {
            /*
            var theResult = dbContext.SYS_USER;
            //return Json(theResult, JsonRequestBehavior.AllowGet);
            return View(theResult.ToList());
           */

            var theResult = dbContext.SYS_USER;
            return View(theResult);
        }

       
        public ActionResult EditSysUser(int id)
        {
            var theResult = dbContext.SYS_USER.Where(a => a.id == id).FirstOrDefault();
            return View(theResult);
        }

        [HttpPost]
        public ActionResult EditSysUser(sys_user form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //dbContext.Entry(form).State = EntityState.Modified;
                    //form.created_at = DateTime.Now;
                    form.updated_at = DateTime.Now;
                    dbContext.Entry(form).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("GetSysUser");
        }
        
        
        public ActionResult DetailsSysUser(int id) 
        {
            var theResult = dbContext.SYS_USER.Where(a => a.id == id).FirstOrDefault();
            return View(theResult);
        }
        

        public ActionResult DeleteSysUser(int id)
        {
            sys_user theResult = dbContext.SYS_USER.Find(id);
            dbContext.SYS_USER.Remove(theResult);
            dbContext.SaveChanges();
            return RedirectToAction("GetSysUser");
        }

        public ActionResult CreateSysUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateSysUser(sys_user form)
        {
            try
            {
                form.created_at = DateTime.Now;
                form.updated_at = DateTime.Now;
                dbContext.SYS_USER.Add(form);
                dbContext.SaveChanges();
            }
            catch(Exception ex){
                throw ex;
            }
            return RedirectToAction("GetSysUser");
        }
    }
}