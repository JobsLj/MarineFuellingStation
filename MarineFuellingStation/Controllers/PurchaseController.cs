﻿using MFS.Controllers.Attributes;
using MFS.Helper;
using MFS.Hubs;
using MFS.Models;
using MFS.Repositorys;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MFS.Controllers
{
    [Route("api/[controller]"), Axios]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseRepository r;
        private readonly ProductRepository p_r;
        private readonly IHostingEnvironment _hostingEnvironment;
        WorkOption option;
        private readonly IHubContext<PrintHub> _hub;
        public PurchaseController(PurchaseRepository repository,ProductRepository p_repository, IOptionsSnapshot<WorkOption> option, IHostingEnvironment env, IHubContext<PrintHub> hub)
        {
            r = repository;
            p_r = p_repository;
            _hostingEnvironment = env;
            //获取 销售单 企业微信应用的AccessToken
            this.option = option.Value;

            _hub = hub;
        }
        [NonAction]
        public async Task SendPrintUnloadAsync(string who, Purchase pu)
        {
            foreach (var connectionId in PrintHub.connections.GetConnections(who))
            {
                await _hub.Clients.Client(connectionId).SendAsync("printunload", pu);
            }
        }
        #region GET
        [HttpGet]
        public ResultJSON<List<Purchase>> Get()
        {
            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = r.GetIncludeProduct().OrderByDescending(p => p.Id).ToList()
            };
        }
        /// <summary>
        /// 分页显示数据
        /// </summary>
        /// <param name="page">第N页</param>
        /// <param name="pageSize">页记录数</param>
        /// <param name="isShowAll">显示全部true|只显示状态为‘未审核’的单</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<List<Purchase>> GetByPager(int page, int pageSize, string sv, bool isShowAll)
        {
            Expression<Func<Purchase, bool>> where = o => 1 == 1;
            if (!isShowAll)
                where = where.And(p => p.State != Purchase.UnloadState.已审核);
            if (!string.IsNullOrEmpty(sv))
                where = where.And(p => p.CarNo.Contains(sv));
            List<Purchase> list;
            list = r.LoadPageList(page, pageSize, out int rCount, true, false, where).Include(p => p.Product).OrderByDescending(p => p.Id).ToList();
            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = list
            };
        }
        [HttpGet("[action]/{id}")]
        public ResultJSON<Purchase> GetDetail(int id)
        {
            Purchase p = r.GetDetail(id);
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = p
            };
        }
        [HttpGet("[action]/{n}")]
        public ResultJSON<List<Purchase>> GetTopN(int n)
        {
            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = r.GetTopNPurchases(n)
            };
        }
        [HttpGet("{sv}")]
        public ResultJSON<List<Purchase>> Get(string sv)
        {
            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = r.GetIncludeProduct().Where(s => s.CarNo.Contains(sv)).ToList()
            };
        }
        [HttpGet("[action]")]
        public ResultJSON<string> PurchaseNo()
        {
            return new ResultJSON<string>
            {
                Code = 0,
                Data = r.GetSerialNumber(r.GetLastPurchaseNo())
            };
        }
        /// <summary>
        /// 获得最近生成的进油单
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<Purchase> LastPurchase()
        {
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = r.GetAllList().OrderByDescending(p => p.Id).FirstOrDefault()
            };
        }
        /// <summary>
        /// 准备卸油操作的单据
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<List<Purchase>> GetReadyUnload()
        {

            return new ResultJSON<List<Purchase>>
            {
                Code = 0,

                Data = r.GetReadyUnload()
            };
        }
        /// <summary>
        /// 根据状态显示所有数据
        /// </summary>
        /// <param name="pus">State状态</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<List<Purchase>> GetAllByState(Purchase.UnloadState pus)
        {

            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = r.GetIncludeProduct().Where(p => p.State == pus).OrderByDescending(p => p.Id).ToList()
            };
        }
        /// <summary>
        /// 根据状态分页显示数据
        /// </summary>
        /// <param name="page">第N页</param>
        /// <param name="pageSize">页记录数</param>
        /// <param name="pus">State状态</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<List<Purchase>> GetByState(int page, int pageSize, Purchase.UnloadState pus)
        {
            
            return new ResultJSON<List<Purchase>>
            {
                Code = 0,
                Data = r.GetByState(page, pageSize, pus)
            };
        }
        /// <summary>
        /// 指定目标推送【陆上卸油单】打印指令
        /// </summary>
        /// <param name="id">Purchase id</param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ResultJSON<Purchase>> PrintUnload(int id, string to)
        {
            Purchase bc = r.GetWithInclude(id);
            await SendPrintUnloadAsync(to, bc);
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = bc
            };
        }
        /// <summary>
        /// 指定目标推送【卸车石化过磅单】打印指令
        /// </summary>
        /// <param name="id">Purchase id</param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ResultJSON<Purchase>> PrintUnloadPond(int id, string to)
        {
            Purchase pu = r.GetWithInclude(id);
            foreach (var connectionId in PrintHub.connections.GetConnections(to))
            {
                await _hub.Clients.Client(connectionId).SendAsync("printunloadpond", pu);
            }
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = pu
            };
        }
        /// <summary>
        /// 从微信服务端远程取得并保存本地
        /// </summary>
        /// <param name="fileId">微信服务端返回的文件Id</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public ResultJSON<string> GetUploadFile(string fileId)
        {
            string fileName = Guid.NewGuid() + ".jpg";
            var filePath = _hostingEnvironment.WebRootPath + @"\upload\";
            bool isSave = FileHelper.SaveFileByWeixin(fileId, filePath, fileName, option.CorpId, option.Secret);
            if (isSave)
                return new ResultJSON<string> { Code = 0, Data = $"/upload/{fileName}" };
            else
                return new ResultJSON<string> { Code = 503, Msg = "上传失败" };
        }
        #endregion
        #region PUT
        [HttpPut("[action]")]
        public ResultJSON<Purchase> ChangeState([FromBody]Purchase p)
        {
            r.CurrentUser = UserName;
            var model = r.Update(p);
            model.LastUpdatedBy = UserName;
            if (p.State == Purchase.UnloadState.完工)
            {
                //推送到“卸油审核”
                this.option.卸油审核AccessToken = AccessTokenContainer.TryGetToken(this.option.CorpId, this.option.卸油审核Secret);
                MassApi.SendTextCard(option.卸油审核AccessToken, option.卸油审核AgentId, "卸油施工结束，请审核更新油仓"
                         , $"<div class=\"gray\">单号：{model.Name}</div>" +
                         $"<div class=\"normal\">车号：{model.CarNo}</div>" +
                         $"<div class=\"normal\">计划数量：{model.Count}吨</div>" +
                         $"<div class=\"normal\">卸仓数量：{model.OilCount}升</div>" +
                         $"<div class=\"normal\">密度：{model.Density}</div>"
                         , $"http://vue.car0774.com/#/purchase/purchase/" + model.Id + "/buyboard", toUser: "@all");
            }
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = model
            };
        }
        
        [HttpPut("[action]")]
        public ResultJSON<Purchase> UnloadRestart(int pid)
        {
            r.CurrentUser = UserName;
            var purchase = r.Get(pid);
            purchase.State = Purchase.UnloadState.已开单;
            purchase.Worker = "";
            r.Save();
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = purchase
            };
        }
        /// <summary>
        /// 审核卸油 设置状态State为已审核
        /// </summary>
        /// <param name="sp">model</param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public ResultJSON<Purchase> AuditingOK([FromBody]Purchase pu)
        {
            pu.State = Purchase.UnloadState.已审核;
            decimal infactTotal = r.UpdateStoreOil(pu);
            string pName = p_r.Get(pu.ProductId).Name;//取得商品名称
            //更新油仓相关数量，平均单价，出入仓记录
            if (infactTotal > 0)
            {
                //推送到“油仓情况”
                this.option.油仓情况AccessToken = AccessTokenContainer.TryGetToken(this.option.CorpId, this.option.油仓情况Secret);
                MassApi.SendTextCard(option.油仓情况AccessToken, option.油仓情况AgentId, "卸油审核成功，已更新油仓油量"
                         , $"<div class=\"gray\">卸油单号：{pu.Name}</div>" +
                         $"<div class=\"normal\">审核人：{UserName}</div>" +
                         $"<div class=\"normal\">商品：{pName}</div>" +
                         $"<div class=\"normal\">计划：{pu.Count}吨</div>" +
                         $"<div class=\"normal\">实际：{infactTotal}升</div>" +
                         $"<div class=\"normal\">密度：{pu.Density}</div>"

                         , $"https://vue.car0774.com/#/purchase/purchase/{pu.Id}/unloadaudit", toUser: "@all");

                return new ResultJSON<Purchase>
                {
                    Code = 0,
                    Data = r.Update(pu)
                };
            }
            else
                return new ResultJSON<Purchase>
                {
                    Code = 500,
                    Msg = "操作失败"
                };
        }
        #endregion
        #region POST
        [HttpPost("[action]")]
        public async Task<ResultJSON<string>> UploadFile([FromForm]IFormFile file)
        {
            if (file != null)
            {
                var extName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                int i = extName.LastIndexOf('.');
                extName = extName.Substring(i);
                string fileName = Guid.NewGuid() + extName;
                var filePath = _hostingEnvironment.WebRootPath + @"\upload\" + fileName;
                await file.SaveAsAsync(filePath);
                return new ResultJSON<string>
                {
                    Code = 0,
                    Data = $"/upload/{fileName}"
                };
            }
            else
            {
                return new ResultJSON<string>
                {
                    Code = 1
                };
            }
        }
        [HttpPost]
        public ResultJSON<Purchase> Post([FromBody]Purchase p)
        {
            //判断是否重复单号
            if (r.Has(pu => pu.Name == p.Name))
                return new ResultJSON<Purchase> { Code = 502 };

            var product = p.Product;
            p.Product = null;
            r.CurrentUser = UserName;
            var result = r.Insert(p);
            
            
            //推送到“进油看板”
            this.option.进油看板AccessToken = AccessTokenContainer.TryGetToken(this.option.CorpId, this.option.进油看板Secret);
            MassApi.SendTextCard(option.进油看板AccessToken, option.进油看板AgentId, $"{UserName}开出了进油计划单"
                     , $"<div class=\"gray\">单号：{result.Name}</div>" +
                     $"<div class=\"normal\">商品：{product.Name}</div>" +
                     $"<div class=\"normal\">运输车号：{result.CarNo}{result.TrailerNo}</div>" +
                     $"<div class=\"normal\">预计到达：{result.ArrivalTime}</div>"
                     , $"https://vue.car0774.com/#/produce/buyboard", toUser: "@all");
            //推送到“陆上卸油”
            this.option.陆上卸油AccessToken = AccessTokenContainer.TryGetToken(this.option.CorpId, this.option.陆上卸油Secret);
            MassApi.SendTextCard(option.陆上卸油AccessToken, option.陆上卸油AgentId, $"{UserName}开出了进油计划单"
                     , $"<div class=\"gray\">单号：{result.Name}</div>" +
                     $"<div class=\"normal\">商品：{product.Name}</div>" +
                     $"<div class=\"normal\">运输车号：{result.CarNo}{result.TrailerNo}</div>" +
                     $"<div class=\"normal\">预计到达：{result.ArrivalTime}</div>"
                     , $"https://vue.car0774.com/#/produce/buyboard", toUser: "@all");
            
            return new ResultJSON<Purchase>
            {
                Code = 0,
                Data = result
            };
        }
        #endregion
        /// <summary>
        /// 作废单据
        /// </summary>
        /// <param name="id">单据id</param>
        /// <param name="delreason">作废原因</param>
        /// <returns></returns>
        #region Delete
        [HttpDelete]
        public ResultJSON<Purchase> Del(int id, string delreason)
        {
            try
            {
                Purchase purchase = r.SetIsDel(id, delreason);

                //推送到“进油看板”
                this.option.进油看板AccessToken = AccessTokenContainer.TryGetToken(this.option.CorpId, this.option.进油看板Secret);
                MassApi.SendTextCard(option.进油看板AccessToken, option.进油看板AgentId, $"{UserName}作废了进油计划单"
                         , $"<div class=\"gray\">单号：{purchase.Name}</div>" +
                         $"<div class=\"normal\">原因：{purchase.DelReason}</div>"
                         , $"https://vue.car0774.com/#/purchase/purchase/" + purchase.Id + "/buyboard", toUser: "@all");

                return new ResultJSON<Purchase> { Code = 0, Data = purchase };
            }
            catch (Exception e)
            {
                return new ResultJSON<Purchase> { Code = 503, Msg = e.Message };
            }
        }
        #endregion
    }
}
