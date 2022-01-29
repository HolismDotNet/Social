namespace Social;

public class CommentController : Controller<Comment, Comment>
{
    public override ReadBusiness<Comment> ReadBusiness => new CommentBusiness();
    
    public override Business<Comment, Comment> Business => new CommentBusiness();
}