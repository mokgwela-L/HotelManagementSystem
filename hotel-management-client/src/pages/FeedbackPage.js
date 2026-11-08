import React, { useState, useEffect } from 'react';
import { reviewAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './FeedbackPage.css';

function FeedbackPage() {
  const navigate = useNavigate();
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  const [reviews, setReviews] = useState([]);
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState({ type: '', text: '' });
  const [filterRating, setFilterRating] = useState('all');

  useEffect(() => {
    if (!currentUser || currentUser.role !== 'Receptionist') {
      alert('Access denied. Receptionist only.');
      navigate('/');
      return;
    }

    fetchReviews();
    fetchStats();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchReviews = async () => {
    setLoading(true);
    try {
      const response = await reviewAPI.getAll();
      setReviews(response.data);
    } catch (error) {
      console.error('Error fetching reviews:', error);
      setMessage({ type: 'error', text: 'Failed to load reviews.' });
    } finally {
      setLoading(false);
    }
  };

  const fetchStats = async () => {
    try {
      const response = await reviewAPI.getStats();
      setStats(response.data);
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  const getFilteredReviews = () => {
    if (filterRating === 'all') {
      return reviews;
    }
    return reviews.filter(r => r.rating === parseInt(filterRating));
  };

  const renderStars = (rating) => {
    return [1, 2, 3, 4, 5].map((star) => (
      <span
        key={star}
        className={`star ${rating >= star ? 'filled' : ''}`}
      >
        ★
      </span>
    ));
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getRatingColor = (rating) => {
    if (rating >= 4) return 'green';
    if (rating === 3) return 'orange';
    return 'red';
  };

  if (!currentUser) {
    return null;
  }

  if (loading) {
    return (
      <div className="feedback-page">
        <div className="feedback-header">
          <h1>📊 Guest Feedback</h1>
          <p>Loading...</p>
        </div>
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Loading feedback...</p>
        </div>
      </div>
    );
  }

  const filteredReviews = getFilteredReviews();

  return (
    <div className="feedback-page">
      <div className="feedback-header">
        <h1>📊 Guest Feedback</h1>
        <p>View and analyze guest reviews</p>
      </div>

      <div className="feedback-container">
        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        {stats && (
          <div className="stats-section">
            <div className="stat-card main-stat">
              <div className="stat-icon">⭐</div>
              <div className="stat-content">
                <h3>{stats.averageRating.toFixed(1)}</h3>
                <p>Average Rating</p>
                <div className="stars-display">
                  {renderStars(Math.round(stats.averageRating))}
                </div>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon">📝</div>
              <div className="stat-content">
                <h3>{stats.totalReviews}</h3>
                <p>Total Reviews</p>
              </div>
            </div>

            <div className="rating-breakdown">
              <h4>Rating Distribution</h4>
              <div className="breakdown-bars">
                {[5, 4, 3, 2, 1].map(rating => {
                  const count = stats[`${['oneStar', 'twoStars', 'threeStars', 'fourStars', 'fiveStars'][rating - 1]}`] || 0;
                  const percentage = stats.totalReviews > 0 ? (count / stats.totalReviews) * 100 : 0;
                  
                  return (
                    <div key={rating} className="breakdown-row">
                      <span className="rating-label">{rating} ★</span>
                      <div className="progress-bar">
                        <div 
                          className={`progress-fill rating-${rating}`}
                          style={{ width: `${percentage}%` }}
                        ></div>
                      </div>
                      <span className="rating-count">{count}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        )}

        <div className="filters-section">
          <button 
            className={`filter-btn ${filterRating === 'all' ? 'active' : ''}`}
            onClick={() => setFilterRating('all')}
          >
            All ({reviews.length})
          </button>
          <button 
            className={`filter-btn ${filterRating === '5' ? 'active' : ''}`}
            onClick={() => setFilterRating('5')}
          >
            5 ★
          </button>
          <button 
            className={`filter-btn ${filterRating === '4' ? 'active' : ''}`}
            onClick={() => setFilterRating('4')}
          >
            4 ★
          </button>
          <button 
            className={`filter-btn ${filterRating === '3' ? 'active' : ''}`}
            onClick={() => setFilterRating('3')}
          >
            3 ★
          </button>
          <button 
            className={`filter-btn ${filterRating === '2' ? 'active' : ''}`}
            onClick={() => setFilterRating('2')}
          >
            2 ★
          </button>
          <button 
            className={`filter-btn ${filterRating === '1' ? 'active' : ''}`}
            onClick={() => setFilterRating('1')}
          >
            1 ★
          </button>
        </div>

        <div className="reviews-section">
          <h2>Reviews ({filteredReviews.length})</h2>

          {filteredReviews.length === 0 ? (
            <div className="no-reviews">
              <p>📭 No reviews found {filterRating !== 'all' && `with ${filterRating} stars`}</p>
            </div>
          ) : (
            <div className="reviews-grid">
              {filteredReviews.map((review) => (
                <div key={review.id} className="review-card">
                  <div className="review-header">
                    <div className="guest-info">
                      <div className="guest-avatar">
                        {review.guestName.split(' ').map(n => n[0]).join('')}
                      </div>
                      <div>
                        <h3>{review.guestName}</h3>
                        <p className="guest-email">{review.guestEmail}</p>
                      </div>
                    </div>
                    <div className={`rating-badge ${getRatingColor(review.rating)}`}>
                      {review.rating} ★
                    </div>
                  </div>

                  <div className="stars-display">
                    {renderStars(review.rating)}
                  </div>

                  {review.comment && (
                    <div className="review-comment">
                      <p>"{review.comment}"</p>
                    </div>
                  )}

                  <div className="review-footer">
                    <span className="review-date">
                      🕒 {formatDate(review.reviewDate)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default FeedbackPage;