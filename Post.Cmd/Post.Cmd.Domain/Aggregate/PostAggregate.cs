using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQRS.Core.Domain;
using Post.Common.Events;

namespace Post.Cmd.Domain.Aggregate
{
    public class PostAggregate : AggregateRoot
    {
        private bool _active;
        private string _author;
        private readonly Dictionary<Guid, Tuple<string, string>> _comment = new();
        public bool Active
        {
            get => _active; set => _active = value;
        }
        public PostAggregate()
        {

        }
        public PostAggregate(Guid id, string author, string message)
        {
            RaiseEvent(new PostCreatedEvent
            {
                Author = author,
                Id = id,
                Message = message,
                DatePosted = DateTimeOffset.Now
            });
        }
        public void Apply(PostCreatedEvent @event)
        {
            _id = @event.Id;
            _author = @event.Author;
            _active = true;
        }
        public void EditMessage(string message)
        {
            if (!_active)
            {
                throw new InvalidOperationException("you cannot edit the message of an inactive post!");
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException($"the value of {nameof(message)} cannot be nuyll or empty");
            }
            RaiseEvent(new MessageUpdatedEvent
            {
                Id = _id,
                Message = message
            });
        }
        public void Apply(MessageUpdatedEvent @event)
        {
            _id = @event.Id;
        }
        public void LikePost()
        {
            if (!_active)
            {
                throw new InvalidOperationException("you can not like an inactive post!");
            }
            RaiseEvent(new PostLikeEvent
            {
                Id = _id
            });
        }
        public void Apply(PostLikeEvent @event)
        {
            _id = @event.Id;
        }
        public void AddComment(string comment, string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("you cannot add a comment of an inactive post!");
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new InvalidOperationException($"the value of {nameof(comment)} cannot be nuyll or empty");
            }
            RaiseEvent(new CommentAddedEvent
            {
                Id = _id,
                CommentId = Guid.NewGuid(),
                Comment = comment,
                Username = username,
                CommentDate = DateTimeOffset.Now
            });
        }
        public void Apply(CommentAddedEvent @event)
        {
            _id = @event.Id;
            _comment.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }
        public void EditComment(Guid commentId, string comment, string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("you cannot add a comment of an inactive Comment!");
            }
            if (!_comment[commentId].Item2.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("you are not allowed to edit a comment that was made by another user!");
            }
            RaiseEvent(new CommentUpdatedEvent
            {
                CommentId = commentId,
                Comment = comment,
                EditDate = DateTimeOffset.Now,
                Username = username,
                Id = _id
            });
        }
        public void Apply(CommentUpdatedEvent @event)
        {
            _id = @event.Id;
            _comment.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.Username));
        }
        public void RemoveComment(Guid commentId, string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("you cannot add a comment of an inactive Comment!");
            }
            if (!_comment[commentId].Item2.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("you are not allowed to remove a comment that was made by another user!");
            }
            RaiseEvent(new CommentRemovedEvent
            {
                Id = _id,
                CommentId = commentId,
            });
        }
        public void Apply(CommentRemovedEvent @event)
        {
            _id = @event.Id;
            _comment.Remove(@event.CommentId);
        }
        public void DeletePost(string username)
        {
            if (!_active)
            {
                throw new InvalidOperationException("the post has already removedf");
            }
            if (!_author.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("you are not allowed to remove a post that was made by another user");
            }
            RaiseEvent(new PostRemovedEvent
            {
                Id = _id
            });
        }
        public void Apply(PostRemovedEvent @event)
        {
            _id = @event.Id;
            _active = false;
        }
    }
}