import React, { useState, useEffect } from 'react';
import { bookingAPI } from '../services/api.js';
import { useNavigate } from 'react-router-dom';
import './MyBookingsPage.css';

function MyBookingsPage() {
  const navigate = useNavigate();
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [message, setMessage] = useState({ type: '', text: '' });
  const [filterStatus, setFilterStatus] = useState('all');

  useEffect(() => {
    if (!currentUser) {
      navigate('/login');
      return;
    }
    fetchMyBookings();
  }, [currentUser, navigate]);

  const fetchMyBookings = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await bookingAPI.getByGuest(currentUser.id);
      setBookings(response.data);
    } catch (err) {
      setError('Failed to load your bookings.');
      console.error('Error fetching bookings:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelBooking = async (bookingId) => {
    if (!window.confirm('Are you sure you want to cancel this booking?')) {
      return;
    }

    setMessage({ type: '', text: '' });

    try {
      await bookingAPI.cancel(bookingId);
      setMessage({ 
        type: 'success', 
        text: 'Booking cancelled successfully!' 
      });
      
      // Refresh bookings
      fetchMyBookings();
    } catch (error) {
      setMessage({ 
        type: 'error', 
        text: 'Failed to cancel booking. Please try again.' 
      });
    }
  };

  const getFilteredBookings = () => {
    if (filterStatus === 'all') {
      return bookings;
    }
    return bookings.filter(b => b.status.toLowerCase() === filterStatus.toLowerCase());
  };

  const getStatusClass = (status) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'status-pending';
      case 'confirmed':
        return 'status-confirmed';
      case 'cancelled':
        return 'status-cancelled';
      case 'completed':
        return 'status-completed';
      default:
        return '';
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  const canCancelBooking = (booking) => {
    return booking.status === 'Pending' || booking.status === 'Confirmed';
  };

  if (loading) {
    return (
      <div className="my-bookings-page">
        <div className="loading">Loading your bookings...</div>
      </div>
    );
  }

  const filteredBookings = getFilteredBookings();

  return (
    <div className="my-bookings-page">
      <div className="my-bookings-header">
        <h1>My Bookings</h1>
        <p>View and manage your hotel reservations</p>
      </div>

      {message.text && (
        <div className={`message ${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="bookings-controls">
        <div className="filters-section">
          <button 
            className={`filter-btn ${filterStatus === 'all' ? 'active' : ''}`}
            onClick={() => setFilterStatus('all')}
          >
            All ({bookings.length})
          </button>
          <button 
            className={`filter-btn ${filterStatus === 'pending' ? 'active' : ''}`}
            onClick={() => setFilterStatus('pending')}
          >
            Pending
          </button>
          <button 
            className={`filter-btn ${filterStatus === 'confirmed' ? 'active' : ''}`}
            onClick={() => setFilterStatus('confirmed')}
          >
            Confirmed
          </button>
          <button 
            className={`filter-btn ${filterStatus === 'completed' ? 'active' : ''}`}
            onClick={() => setFilterStatus('completed')}
          >
            Completed
          </button>
          <button 
            className={`filter-btn ${filterStatus === 'cancelled' ? 'active' : ''}`}
            onClick={() => setFilterStatus('cancelled')}
          >
            Cancelled
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="bookings-container">
        {filteredBookings.length === 0 ? (
          <div className="no-bookings">
            <h3>📭 No bookings found</h3>
            <p>You haven't made any bookings yet.</p>
            <a href="/bookings" className="create-booking-link">Make a Booking</a>
          </div>
        ) : (
          <div className="bookings-grid">
            {filteredBookings.map((booking) => (
              <div key={booking.id} className="booking-card">
                <div className="booking-header">
                  <h3>Booking #{booking.id}</h3>
                  <span className={`status-badge ${getStatusClass(booking.status)}`}>
                    {booking.status}
                  </span>
                </div>

                <div className="booking-details">
                  <div className="detail-row">
                    <span className="icon">🏨</span>
                    <div>
                      <strong>Room:</strong>
                      <p>Room {booking.roomNumber}</p>
                    </div>
                  </div>

                  <div className="detail-row">
                    <span className="icon">📅</span>
                    <div>
                      <strong>Check-In:</strong>
                      <p>{formatDate(booking.checkInDate)}</p>
                    </div>
                  </div>

                  <div className="detail-row">
                    <span className="icon">📅</span>
                    <div>
                      <strong>Check-Out:</strong>
                      <p>{formatDate(booking.checkOutDate)}</p>
                    </div>
                  </div>

                  <div className="detail-row">
                    <span className="icon">💰</span>
                    <div>
                      <strong>Total Price:</strong>
                      <p>R{booking.totalPrice.toFixed(2)}</p>
                    </div>
                  </div>

                  <div className="detail-row">
                    <span className="icon">🕒</span>
                    <div>
                      <strong>Booked On:</strong>
                      <p>{formatDate(booking.bookingDate)}</p>
                    </div>
                  </div>

                  {booking.specialRequests && (
                    <div className="detail-row special-requests">
                      <span className="icon">📝</span>
                      <div>
                        <strong>Special Requests:</strong>
                        <p>{booking.specialRequests}</p>
                      </div>
                    </div>
                  )}
                </div>

                {canCancelBooking(booking) && (
                  <div className="booking-actions">
                    <button 
                      className="cancel-btn"
                      onClick={() => handleCancelBooking(booking.id)}
                    >
                      ✕ Cancel Booking
                    </button>
                  </div>
                )}

                {booking.status === 'Cancelled' && (
                  <div className="cancelled-note">
                    ❌ This booking has been cancelled
                  </div>
                )}

                {booking.status === 'Completed' && (
                  <div className="completed-note">
                    ✅ Thank you for staying with us!
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default MyBookingsPage;