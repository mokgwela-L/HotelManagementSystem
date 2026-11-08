import React, { useState, useEffect } from 'react';
import { bookingAPI, roomAPI } from '../services/api.js';
import { useNavigate } from 'react-router-dom';
import './BookingsPage.css';

function BookingsPage() {
  const navigate = useNavigate();
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  // Redirect if not logged in
  useEffect(() => {
    if (!currentUser) {
      alert('Please login to make a booking');
      navigate('/login');
    }
  }, [currentUser, navigate]);

  const [rooms, setRooms] = useState([]);
  const [formData, setFormData] = useState({
    guestId: currentUser?.id || '', // Pre-fill with current user
    roomId: '',
    checkInDate: '',
    checkOutDate: '',
    specialRequests: ''
  });
  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);
  const [calculatedPrice, setCalculatedPrice] = useState(0);

  useEffect(() => {
    fetchRooms();
  }, []);

  useEffect(() => {
    calculatePrice();
  }, [formData.roomId, formData.checkInDate, formData.checkOutDate, rooms]);

  const fetchRooms = async () => {
    try {
      const roomsResponse = await roomAPI.getAvailable();
      setRooms(roomsResponse.data);
    } catch (error) {
      console.error('Error fetching data:', error);
      setMessage({ type: 'error', text: 'Failed to load rooms.' });
    }
  };

  const calculatePrice = () => {
    if (formData.roomId && formData.checkInDate && formData.checkOutDate) {
      const room = rooms.find(r => r.id === parseInt(formData.roomId));
      if (room) {
        const checkIn = new Date(formData.checkInDate);
        const checkOut = new Date(formData.checkOutDate);
        const nights = Math.ceil((checkOut - checkIn) / (1000 * 60 * 60 * 24));
        
        if (nights > 0) {
          setCalculatedPrice(nights * room.pricePerNight);
        } else {
          setCalculatedPrice(0);
        }
      }
    }
  };

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const validateForm = () => {
    if (!formData.roomId) {
      setMessage({ type: 'error', text: 'Please select a room' });
      return false;
    }
    if (!formData.checkInDate) {
      setMessage({ type: 'error', text: 'Check-in date is required' });
      return false;
    }
    if (!formData.checkOutDate) {
      setMessage({ type: 'error', text: 'Check-out date is required' });
      return false;
    }

    const checkIn = new Date(formData.checkInDate);
    const checkOut = new Date(formData.checkOutDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (checkIn < today) {
      setMessage({ type: 'error', text: 'Check-in date cannot be in the past' });
      return false;
    }

    if (checkOut <= checkIn) {
      setMessage({ type: 'error', text: 'Check-out date must be after check-in date' });
      return false;
    }

    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage({ type: '', text: '' });

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const bookingData = {
        guestId: currentUser.id, // Use current user's ID
        roomId: parseInt(formData.roomId),
        checkInDate: formData.checkInDate,
        checkOutDate: formData.checkOutDate,
        specialRequests: formData.specialRequests
      };

      const response = await bookingAPI.create(bookingData);
      
      setMessage({ 
        type: 'success', 
        text: `Booking created successfully! Booking ID: ${response.data.id}. Total: R${response.data.totalPrice}` 
      });
      
      // Clear form
      setFormData({
        guestId: currentUser.id,
        roomId: '',
        checkInDate: '',
        checkOutDate: '',
        specialRequests: ''
      });
      setCalculatedPrice(0);

      fetchRooms();
    } catch (error) {
      if (error.response && error.response.status === 409) {
        setMessage({ 
          type: 'error', 
          text: 'This room is already booked for the selected dates!' 
        });
      } else if (error.response && error.response.status === 404) {
        setMessage({ 
          type: 'error', 
          text: error.response.data.message || 'Guest or Room not found.' 
        });
      } else if (error.response && error.response.status === 400) {
        setMessage({ 
          type: 'error', 
          text: error.response.data.message || 'Invalid booking data.' 
        });
      } else {
        setMessage({ 
          type: 'error', 
          text: 'Failed to create booking. Please try again.' 
        });
      }
    } finally {
      setLoading(false);
    }
  };

  const getTodayDate = () => {
    const today = new Date();
    return today.toISOString().split('T')[0];
  };

  const getTomorrowDate = () => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  };

  if (!currentUser) {
    return null;
  }

  return (
    <div className="bookings-page">
      <div className="bookings-header">
        <h1>Make a Booking</h1>
        <p>Reserve your perfect room today</p>
      </div>

      <div className="booking-container">
        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        <form onSubmit={handleSubmit} className="booking-form">
          {/* Guest info display (not editable) */}
          <div className="form-group">
            <label>Booking for:</label>
            <div className="guest-info-display">
              <p><strong>{currentUser.firstName} {currentUser.lastName}</strong></p>
              <p>{currentUser.email}</p>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="roomId">Select Room *</label>
              <select
                id="roomId"
                name="roomId"
                value={formData.roomId}
                onChange={handleChange}
                required
              >
                <option value="">-- Choose a room --</option>
                {rooms.map((room) => (
                  <option key={room.id} value={room.id}>
                    Room {room.roomNumber} - {room.roomType} (R{room.pricePerNight}/night)
                  </option>
                ))}
              </select>
              {rooms.length === 0 && (
                <p className="helper-text error-text">
                  No available rooms found.
                </p>
              )}
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="checkInDate">Check-In Date *</label>
              <input
                type="date"
                id="checkInDate"
                name="checkInDate"
                value={formData.checkInDate}
                onChange={handleChange}
                min={getTodayDate()}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="checkOutDate">Check-Out Date *</label>
              <input
                type="date"
                id="checkOutDate"
                name="checkOutDate"
                value={formData.checkOutDate}
                onChange={handleChange}
                min={formData.checkInDate || getTomorrowDate()}
                required
              />
            </div>
          </div>

          {calculatedPrice > 0 && (
            <div className="price-display">
              <h3>Estimated Total: R{calculatedPrice.toFixed(2)}</h3>
              <p>
                {Math.ceil((new Date(formData.checkOutDate) - new Date(formData.checkInDate)) / (1000 * 60 * 60 * 24))} night(s)
              </p>
            </div>
          )}

          <div className="form-group">
            <label htmlFor="specialRequests">Special Requests (Optional)</label>
            <textarea
              id="specialRequests"
              name="specialRequests"
              value={formData.specialRequests}
              onChange={handleChange}
              placeholder="e.g., Late check-in, extra towels, ground floor room..."
              maxLength="500"
              rows="4"
            />
          </div>

          <button 
            type="submit" 
            className="submit-btn"
            disabled={loading || rooms.length === 0}
          >
            {loading ? 'Creating Booking...' : 'Confirm Booking'}
          </button>
        </form>

        <div className="booking-info">
          <h3>📋 Booking Information</h3>
          <ul>
            <li>✅ Check-in time: 3:00 PM</li>
            <li>✅ Check-out time: 11:00 AM</li>
            <li>✅ Cancellation policy: Free cancellation up to 24 hours before check-in</li>
            <li>✅ Payment: Pay at the hotel upon arrival</li>
            <li>✅ Valid ID required at check-in</li>
          </ul>
        </div>
      </div>
    </div>
  );
}

export default BookingsPage;