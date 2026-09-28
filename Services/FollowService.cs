using System;
using System.Threading.Tasks;
using Thinkfeed.Models;
using Thinkfeed.Repositories.Interfaces;
using Thinkfeed.Services.Interfaces;

namespace Thinkfeed.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task ToggleAsync(string followerId, string followingId)
        {
            var existing = await _followRepository.GetAsync(followerId, followingId);
            if (existing != null)
            {
                _followRepository.Remove(existing);
            }
            else
            {
                var follow = new Follow
                {
                    FollowerId = followerId,
                    FollowingId = followingId,
                    CreatedAt = DateTime.Now
                };
                await _followRepository.AddAsync(follow);
            }
            await _followRepository.SaveAsync();
        }

        public async Task<int> GetFollowerCountAsync(string userId)
        {
            return await _followRepository.CountFollowersAsync(userId);
        }

        public async Task<int> GetFollowingCountAsync(string userId)
        {
            return await _followRepository.CountFollowingAsync(userId);
        }
    }
}
