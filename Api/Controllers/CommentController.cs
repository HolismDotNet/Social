using Holism.Social.Business;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Social.UserApi.Controllers
{
    public class CommentController : DefaultController
    {
        string ;

        string ;

        public CommentController(string  = null, string  = null)
        {
            this. = ;
            this. = ;
        }

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
            ((CommentBusiness)Business).ToggleApprovedState(id);
            return OkJson();
        }

        [HttpPost]
        public IActionResult ApproveItems(List<long> ids)
        {
            ((CommentBusiness)Business).ApproveItems(ids);
            return OkJson();
        }

        [HttpPost]
        public IActionResult DisapproveItems(List<long> ids)
        {
            ((CommentBusiness)Business).DisapproveItems(ids);
            return OkJson();
        }
    }
}
