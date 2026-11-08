import React, { useState, useEffect } from 'react';
import { reviewAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './RatingsPage.css';

function RatingsPage() {
  const navigate = useNavigate();
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  const [rating, setRating] = useState(0);
  const [hoverRating, setHoverRating] = useState(0);
  const [comment, setComment] = useState('');
  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);
  const [hasReviewed, setHasReviewed] = useState(false);
  const [existingReview, setExistingReview] = useState(null);

  useEffect(() => {
    if (!currentUser) {
      navigate('/login');
      return;
    }

    // Check if user already submitted a review
    checkExistingReview();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const checkExistingReview = async () => {
    try {
      const response = await reviewAPI.getByGuest(currentUser.id);
      if (response.data && response.data.length > 0) {
        setHasReviewed(true);
        setExistingReview(response.data[0]);
      }
    } catch (error) {
      console.error('Error checking existing review:', error);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage({ type: '', text: '' });

    if (rating === 0) {
      setMessage({ type: 'error', text: 'Please select a star rating' });
      return;
    }

    setLoading(true);

    try {
      const reviewData = {
        guestId: currentUser.id,
        rating: rating,
        comment: comment.trim() || null
      };

      await reviewAPI.create(reviewData);
      
      setMessage({ 
        type: 'success', 
        text: 'Thank you for your feedback! Your review has been submitted.' 
      });

      // Reset form and mark as reviewed
      setRating(0);
      setComment('');
      setHasReviewed(true);
      checkExistingReview();
    } catch (error) {
      if (error.response && error.response.status === 409) {
        setMessage({ 
          type: 'error', 
          text: 'You have already submitted a review.' 
        });
        setHasReviewed(true);
      } else {
        setMessage({ 
          type: 'error', 
          text: 'Failed to submit review. Please try again.' 
        });
      }
    } finally {
      setLoading(false);
    }
  };

  const renderStars = (isInput = false) => {
    return [1, 2, 3, 4, 5].map((star) => {
      if (isInput) {
        return (
          <span
            key={star}
            className={`star ${(hoverRating || rating) >= star ? 'filled' : ''}`}
            onClick={() => setRating(star)}
            onMouseEnter={() => setHoverRating(star)}
            onMouseLeave={() => setHoverRating(0)}
          >
            ★
          </span>
        );
      } else {
        return (
          <span
            key={star}
            className={`star ${existingReview && existingReview.rating >= star ? 'filled' : ''} readonly`}
          >
            ★
          </span>
        );
      }
    });
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

  if (!currentUser) {
    return null;
  }

  return (
    <div className="ratings-page">
      <div className="ratings-header">
        <h1>⭐ Rate Your Experience</h1>
        <p>Your feedback helps us improve our service</p>
      </div>

      <div className="ratings-container">
        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        {!hasReviewed ? (
          <div className="rating-form-card">
            <h2>How was your stay?</h2>
            <p className="subtitle">Rate your overall experience at our hotel</p>

            <form onSubmit={handleSubmit} className="rating-form">
              <div className="star-rating-section">
                <label>Your Rating *</label>
                <div className="stars-input">
                  {renderStars(true)}
                </div>
                {rating > 0 && (
                  <p className="rating-text">
                    {rating === 5 && '⭐ Excellent!'}
                    {rating === 4 && '⭐ Very Good'}
                    {rating === 3 && '⭐ Good'}
                    {rating === 2 && '⭐ Fair'}
                    {rating === 1 && '⭐ Poor'}
                  </p>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="comment">
                  Your Comments (Optional)
                </label>
                <textarea
                  id="comment"
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="Tell us about your experience... (optional)"
                  maxLength="1000"
                  rows="6"
                />
                <p className="char-count">{comment.length}/1000 characters</p>
              </div>

              <button 
                type="submit" 
                className="submit-btn"
                disabled={loading || rating === 0}
              >
                {loading ? 'Submitting...' : 'Submit Review'}
              </button>
            </form>
          </div>
        ) : (
          <div className="existing-review-card">
            <h2>✅ Thank You for Your Feedback!</h2>
            <p className="subtitle">You have already submitted a review</p>

            {existingReview && (
              <div className="review-display">
                <div className="review-rating">
                  <div className="stars-display">
                    {renderStars(false)}
                  </div>
                  <span className="rating-number">{existingReview.rating}/5</span>
                </div>

                {existingReview.comment && (
                  <div className="review-comment">
                    <p className="comment-label">Your Comment:</p>
                    <p className="comment-text">"{existingReview.comment}"</p>
                  </div>
                )}

                <p className="review-date">
                  Submitted on {formatDate(existingReview.reviewDate)}
                </p>
              </div>
            )}

            <div className="info-box">
              <p>
                💡 <strong>Note:</strong> You can only submit one review per account. 
                If you'd like to update your feedback, please contact our reception desk.
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default RatingsPage;