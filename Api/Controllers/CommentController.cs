using Holism.Api.Controllers;
using Holism.Business;
using Holism.Social.Business;
using Holism.Social.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Social.Api.Controllers
{
    public class CommentController : ReadController<Comment>
    {        
        public override ReadBusiness<Comment> ReadBusiness => new CommentBusiness();

        [HttpPost]
        public IActionResult ToggleLike(Guid commentGuid)
        {
            new LikeBusiness().ToggleLike(UserGuid, CommentBusiness.EntityType, commentGuid);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ToggleDislike(Guid commentGuid)
        {
            new DislikeBusiness().ToggleDislike(UserGuid, CommentBusiness.EntityType, commentGuid);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ToggleApprovedState(long id)
        {
            //new CommentBusiness.ToggleApprovedState(id);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ApproveItems(List<long> ids)
        {
            //new CommentBusiness.ApproveItems(ids);
            return OkJson();
        }

        [HttpPost]
        public IActionResult DisapproveItems(List<long> ids)
        {
            //new CommentBusiness.DisapproveItems(ids);
            return OkJson();
        }
    }
}
