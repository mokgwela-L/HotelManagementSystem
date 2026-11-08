import React, { useState, useEffect } from 'react';
import { bookingAPI } from '../services/api.js';
import './DashboardPage.css';

function DashboardPage() {
  const [bookings, setBookings] = useState([]);
  const [filteredBookings, setFilteredBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [message, setMessage] = useState({ type: '', text: '' });
  const [filterStatus, setFilterStatus] = useState('all');
  const [selectedBooking, setSelectedBooking] = useState(null);

  useEffect(() => {
    fetchBookings();
  }, []);

  const fetchBookings = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await bookingAPI.getAll();
      setBookings(response.data);
      setFilteredBookings(response.data);
    } catch (err) {
      setError('Failed to load bookings. Please try again later.');
      console.error('Error fetching bookings:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = async (status) => {
    setFilterStatus(status);
    setLoading(true);
    setError('');

    try {
      if (status === 'all') {
        const response = await bookingAPI.getAll();
        setFilteredBookings(response.data);
      } else {
        const response = await bookingAPI.getByStatus(status);
        setFilteredBookings(response.data);
      }
    } catch (err) {
      setError('Failed to filter bookings.');
      console.error('Error filtering bookings:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusUpdate = async (bookingId, newStatus) => {
    setMessage({ type: '', text: '' });
    
    try {
      await bookingAPI.updateStatus(bookingId, newStatus);
      setMessage({ 
        type: 'success', 
        text: `Booking status updated to ${newStatus}` 
      });
      
      // Refresh bookings
      await fetchBookings();
      
      // Reapply filter if needed
      if (filterStatus !== 'all') {
        await handleFilterChange(filterStatus);
      }
    } catch (error) {
      setMessage({ 
        type: 'error', 
        text: 'Failed to update booking status.' 
      });
      console.error('Error updating status:', error);
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
        text: 'Booking cancelled successfully' 
      });
      
      // Refresh bookings
      await fetchBookings();
      
      // Reapply filter if needed
      if (filterStatus !== 'all') {
        await handleFilterChange(filterStatus);
      }
    } catch (error) {
      setMessage({ 
        type: 'error', 
        text: 'Failed to cancel booking.' 
      });
      console.error('Error cancelling booking:', error);
    }
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

  const getStats = () => {
    return {
      total: bookings.length,
      pending: bookings.filter(b => b.status === 'Pending').length,
      confirmed: bookings.filter(b => b.status === 'Confirmed').length,
      cancelled: bookings.filter(b => b.status === 'Cancelled').length,
      completed: bookings.filter(b => b.status === 'Completed').length,
    };
  };

  if (loading && bookings.length === 0) {
    return (
      <div className="dashboard-page">
        <div className="loading">Loading dashboard...</div>
      </div>
    );
  }

  const stats = getStats();

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <h1>Bookings Dashboard</h1>
        <p>Manage all hotel reservations</p>
      </div>

      {message.text && (
        <div className={`message ${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="stats-section">
        <div className="stat-card">
          <h3>{stats.total}</h3>
          <p>Total Bookings</p>
        </div>
        <div className="stat-card pending">
          <h3>{stats.pending}</h3>
          <p>Pending</p>
        </div>
        <div className="stat-card confirmed">
          <h3>{stats.confirmed}</h3>
          <p>Confirmed</p>
        </div>
        <div className="stat-card completed">
          <h3>{stats.completed}</h3>
          <p>Completed</p>
        </div>
        <div className="stat-card cancelled">
          <h3>{stats.cancelled}</h3>
          <p>Cancelled</p>
        </div>
      </div>

      <div className="filters-section">
        <button 
          className={`filter-btn ${filterStatus === 'all' ? 'active' : ''}`}
          onClick={() => handleFilterChange('all')}
        >
          All Bookings
        </button>
        <button 
          className={`filter-btn ${filterStatus === 'Pending' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Pending')}
        >
          Pending
        </button>
        <button 
          className={`filter-btn ${filterStatus === 'Confirmed' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Confirmed')}
        >
          Confirmed
        </button>
        <button 
          className={`filter-btn ${filterStatus === 'Completed' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Completed')}
        >
          Completed
        </button>
        <button 
          className={`filter-btn ${filterStatus === 'Cancelled' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Cancelled')}
        >
          Cancelled
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="bookings-container">
        {filteredBookings.length === 0 ? (
          <div className="no-bookings">
            <p>No bookings found.</p>
            <a href="/bookings" className="create-booking-link">Create a Booking</a>
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
                    <span className="icon">👤</span>
                    <div>
                      <strong>Guest:</strong>
                      <p>{booking.guestName}</p>
                    </div>
                  </div>

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

                <div className="booking-actions">
                  {booking.status === 'Pending' && (
                    <>
                      <button 
                        className="action-btn confirm-btn"
                        onClick={() => handleStatusUpdate(booking.id, 'Confirmed')}
                      >
                        ✓ Confirm
                      </button>
                      <button 
                        className="action-btn cancel-btn"
                        onClick={() => handleCancelBooking(booking.id)}
                      >
                        ✕ Cancel
                      </button>
                    </>
                  )}
                  
                  {booking.status === 'Confirmed' && (
                    <>
                      <button 
                        className="action-btn complete-btn"
                        onClick={() => handleStatusUpdate(booking.id, 'Completed')}
                      >
                        ✓ Complete
                      </button>
                      <button 
                        className="action-btn cancel-btn"
                        onClick={() => handleCancelBooking(booking.id)}
                      >
                        ✕ Cancel
                      </button>
                    </>
                  )}

                  {(booking.status === 'Cancelled' || booking.status === 'Completed') && (
                    <div className="action-note">
                      {booking.status === 'Completed' ? '✓ Booking completed' : '✕ Booking cancelled'}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default DashboardPage;