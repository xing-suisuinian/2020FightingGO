using _2020FightingGO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dingding;
using Dingding.Model;
using Newtonsoft;
using System.Xml;

namespace _2020FightingGO.Controllers
{
    public class FightingController : Controller
    {
		Fighting2020Entities db = new Fighting2020Entities();


        // GET: Fighting
        public ActionResult Index()
        {
            return View();
        }
		public ActionResult Login()
		{
			return View();
		}
		public ActionResult home()
		{
			return View();
		}

		public ActionResult Driver()
		{
			return View();
		}
		public ActionResult ViewList()
		{
			return View();
		}
		public ActionResult ViewMyList()
		{
			return View();
		}
		public ActionResult ViewMyJoinList()
		{
			return View();
		}

		public JsonResult AddSave(T_MainInfo  model)
		{
			DateTime now = DateTime.Now;
			DateTime minTime = model.DepartureTime.AddMinutes(-15);
			DateTime maxTime = model.DepartureTime.AddMinutes(15);
			if (model.DepartureTime < now)
			{
				return Json(new { state = "Fail", msg = "请发布时间大于现在的行程" });
			}
			T_MainInfo exisModel= db.T_MainInfo.FirstOrDefault(a => a.Driver == model.Driver && a.DepartureTime > minTime && a.DepartureTime < maxTime);
			if (exisModel == null)
			{
				model.FreeSeat = model.SeatNumber;
				model.PostTime = DateTime.Now;

				db.T_MainInfo.Add(model);
				try
				{
					db.SaveChanges();
					return Json(new { state = "Success", msg = "保存成功" });
				}
				catch (Exception e)
				{
					return Json(new { state = "Fail", msg = e.Message });
				}
			}
			else
			{
				return Json(new { state = "Fail", msg = "您在该时间点附近已建立行程 ，请合理建立行程安排" });
			}
			
			
		}
		//
		public JsonResult JoinSave(int ID)
		{
			string Passenger = Server.UrlDecode(Request.Cookies["name"].Value);
			string Tel = Server.UrlDecode(Request.Cookies["mobile"].Value);
			T_MainDetail existMainDetail = db.T_MainDetail.FirstOrDefault(a => a.PID == ID && a.Passenger == Passenger);
			if (existMainDetail != null)
			{
				return Json(new { state = "Fail", msg ="您已加入该行程，请勿重复哦" });
			}
			T_MainInfo maininfo = db.T_MainInfo.Find(ID);
			if (maininfo.FreeSeat == 0)
			{
				return Json(new { state = "Fail", msg = "小伙伴已经满了哦，请选择其他行程加入" });
			}
			maininfo.FreeSeat = maininfo.FreeSeat - 1;
			if (maininfo.Passengers == null)
			{
				maininfo.Passengers = Passenger;
			}
			else
			{
				maininfo.Passengers = maininfo.Passengers+","+Passenger;
			}
			
			db.Entry<T_MainInfo>(maininfo).State = System.Data.Entity.EntityState.Modified;
			
			T_MainDetail mainDetail = new T_MainDetail()
			{
				Passenger = Passenger,
				PostTime = DateTime.Now,
				PID=ID,
				Tel= Tel
			};
			db.T_MainDetail.Add(mainDetail);
							
			try
			{
				db.SaveChanges();
				return Json(new { state = "Success", msg = "保存成功,记得及时与车主取得联系哦" });
			}
			catch (Exception e)
			{
				return Json(new { state = "Fail", msg = e.Message });
			}
			


		}

		public JsonResult CancelSave(int ID)
		{
			string name = Server.UrlDecode(Request.Cookies["name"].Value);
			//string name = "黄幸&夕影";
			T_MainInfo maininfo = db.T_MainInfo.Find(ID);
			if (name == maininfo.Driver)
			{
				maininfo.IsCancel = 1;
			}
			
			else
			{
				return Json(new { state = "Fail", msg = "不允许取消他人发布的行程"});
			}
			db.Entry<T_MainInfo>(maininfo).State = System.Data.Entity.EntityState.Modified;

			
			

			try
			{
				db.SaveChanges();
				return Json(new { state = "Success", msg = "保存成功,请记得与相关小伙伴联系，通知您已取消本次行程哦" });
			}
			catch (Exception e)
			{
				return Json(new { state = "Fail", msg = e.Message });
			}
		}
		//
		public JsonResult CancelJoinSave(int ID)
		{
			string name = Server.UrlDecode(Request.Cookies["name"].Value);
			//string name = "黄幸&夕影";
			T_MainInfo maininfo = db.T_MainInfo.Find(ID);
			maininfo.FreeSeat = maininfo.FreeSeat + 1;
			maininfo.Passengers = maininfo.Passengers.Replace(name, "").Replace(",,", ",");
			T_MainDetail deleteMainDetail = db.T_MainDetail.FirstOrDefault(a => a.Passenger == name && a.PID == ID);
			db.T_MainDetail.Remove(deleteMainDetail);
			db.Entry<T_MainInfo>(maininfo).State = System.Data.Entity.EntityState.Modified;
			try
			{
				db.SaveChanges();
				return Json(new { state = "Success", msg = "保存成功,请记得与相关小伙伴联系，通知您已取消本次行程哦" });
			}
			catch (Exception e)
			{
				return Json(new { state = "Fail", msg = e.Message });
			}
		}
		public JsonResult getList(int page,int pagesize, int IsGaoQiao,string DepartureTime,string Destination,string DeparturePlace,string queryStr,int isMy=0, int isMyJoin=0)
		{
			string name = Server.UrlDecode(Request.Cookies["name"].Value);
			//string name = "黄幸&夕影";
			DateTime now = DateTime.Now.AddMinutes(-5);
			IQueryable<T_MainInfo> queryData = db.T_MainInfo.Where(a=>a.DepartureTime> now && a.FreeSeat>0);
			//if (IsGaoQiao == 1)
			//{
			//	queryData = queryData.Where(a => a.IsGaoQiao == IsGaoQiao);
			//}
			if (!string.IsNullOrWhiteSpace(queryStr))
			{
				queryData = queryData.Where(a => a.DeparturePlace.Contains(queryStr)|| a.Destination.Contains(queryStr)||a.Driver.Contains(queryStr));
			}
			if (isMy == 1)
			{
				queryData = queryData.Where(a=>a.Driver== name);
			}
			if (isMyJoin == 1)
			{
				queryData = queryData.Where(a => a.Passengers.Contains(name));
			}

			List<T_MainInfo> list = queryData.OrderBy(a =>new { a.DepartureTime,a.IsCancel } ).Skip(page * pagesize).Take(pagesize).ToList();
			return Json(list,JsonRequestBehavior.AllowGet);
		}
		//getDetail
		public JsonResult getDetail(int ID)
		{
			
			IQueryable<T_MainDetail> queryData = db.T_MainDetail.Where(a => a.PID ==ID);
			

			List<T_MainDetail> list = queryData.ToList();
			return Json(list, JsonRequestBehavior.AllowGet);
		}
		public JsonResult LoginIn(string code= "c5c4f7fdd63d3e7c9c32cc9f9f1944b0")
		{

			Dingding.Service.DingdingService dingdingService = new Dingding.Service.DingdingService();
			try
			{
				Access_UserInfo access_UserInfo = dingdingService.GetUserInfo(code);
				try
				{
					Dingding_User dinging_user = dingdingService.GetUeser(access_UserInfo.userid);
					//string str = "{\"errcode\":0,\"unionid\":\"GzEc4E7CcbBI9AWawhIuPAiEiE\",\"openId\":\"GzEc4E7CcbBI9AWawhIuPAiEiE\",\"roles\":[{\"id\":285034480,\"name\":\"子管理员\",\"groupName\":\"默认\",\"type\":102}],\"remark\":\"2752339200\",\"userid\":\"03136562681284148\",\"isLeaderInDepts\":\"{ 62771015:false}\",\"isBoss\":false,\"hiredDate\":1462636800000,\"isSenior\":false,\"tel\":\"8025\",\"department\":[62771015],\"workPlace\":\"总部线下\",\"email\":\"xiying @googhushi.com\",\"orderInDepts\":\"{ 62771015:177917621779460510}\",\"dingId\":\"$:LWCP_v1:$2jg6k1cWiFRb5RpSKCyYGQ == \",\"mobile\":\"15616268674\",\"errmsg\":\"ok\",\"active\":true,\"avatar\":\"https://static-legacy.dingtalk.com/media/lADPDgQ9qc80Hl_NA-jNAps_667_1000.jpg\",\"isAdmin\":true,\"tags\":{},\"isHide\":false,\"jobnumber\":\"100502\",\"name\":\"黄幸&夕影\",\"extattr\":{\"QQ\":\"478289959\",\"所属公司主体\":\"可孚科技\",\"入职推荐人\":\"无\"},\"stateCode\":\"86\",\"position\":\"软件工程师\"}";
					// x= Newtonsoft.Json.JsonConvert.DeserializeObject<Dingding_User>(str);

					Response.Cookies["name"].Value = dinging_user.name;
					Response.Cookies["mobile"].Value = Server.UrlEncode(dinging_user.mobile);
				}
				catch (Exception e)
				{
					return Json(e.Message + "002",JsonRequestBehavior.AllowGet);
				}
				
			}
			catch (Exception e)
			{
				return Json(e.Message + "001", JsonRequestBehavior.AllowGet);
			}
			

			//Response.Cookies["name"].Value = Server.UrlEncode("admin");
			//Response.Cookies["mobile"].Value = Server.UrlEncode("1588888888");
			return Json(1, JsonRequestBehavior.AllowGet);
		}

		public JsonResult LoginInX(string code)
		{
			return Json(1, JsonRequestBehavior.AllowGet)
;		}
	}
}