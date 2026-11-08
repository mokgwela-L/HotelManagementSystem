import React, { useState, useEffect } from 'react';
import { roomAPI } from '../services/api.js';
import './RoomsPage.css';

function RoomsPage() {
  const [rooms, setRooms] = useState([]);
  const [filteredRooms, setFilteredRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filterType, setFilterType] = useState('all');

  useEffect(() => {
    fetchRooms();
  }, []);

  const fetchRooms = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await roomAPI.getAll();
      setRooms(response.data);
      setFilteredRooms(response.data);
    } catch (err) {
      setError('Failed to load rooms. Please try again later.');
      console.error('Error fetching rooms:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = async (type) => {
    setFilterType(type);
    setLoading(true);
    setError('');

    try {
      if (type === 'all') {
        const response = await roomAPI.getAll();
        setFilteredRooms(response.data);
      } else if (type === 'available') {
        const response = await roomAPI.getAvailable();
        setFilteredRooms(response.data);
      } else {
        const response = await roomAPI.getByType(type);
        setFilteredRooms(response.data);
      }
    } catch (err) {
      setError('Failed to filter rooms.');
      console.error('Error filtering rooms:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="rooms-page">
        <div className="loading">Loading rooms...</div>
      </div>
    );
  }

  return (
    <div className="rooms-page">
      <div className="rooms-header">
        <h1>Our Rooms</h1>
        <p>Choose from our comfortable and luxurious accommodations</p>
      </div>

      <div className="filters-section">
        <button 
          className={`filter-btn ${filterType === 'all' ? 'active' : ''}`}
          onClick={() => handleFilterChange('all')}
        >
          All Rooms
        </button>
        <button 
          className={`filter-btn ${filterType === 'available' ? 'active' : ''}`}
          onClick={() => handleFilterChange('available')}
        >
          Available Only
        </button>
        <button 
          className={`filter-btn ${filterType === 'Single' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Single')}
        >
          Single
        </button>
        <button 
          className={`filter-btn ${filterType === 'Double' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Double')}
        >
          Double
        </button>
        <button 
          className={`filter-btn ${filterType === 'Suite' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Suite')}
        >
          Suite
        </button>
        <button 
          className={`filter-btn ${filterType === 'Deluxe' ? 'active' : ''}`}
          onClick={() => handleFilterChange('Deluxe')}
        >
          Deluxe
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="rooms-grid">
        {filteredRooms.length === 0 ? (
          <div className="no-rooms">
            <p>No rooms found matching your criteria.</p>
          </div>
        ) : (
          filteredRooms.map((room) => (
            <div key={room.id} className="room-card">
              <div className="room-image">
                <div className="room-type-badge">{room.roomType}</div>
                {room.isAvailable ? (
                  <div className="status-badge available">Available</div>
                ) : (
                  <div className="status-badge unavailable">Booked</div>
                )}
              </div>
              
              <div className="room-details">
                <h3>Room {room.roomNumber}</h3>
                <p className="room-description">{room.description}</p>
                
                <div className="room-info">
                  <div className="info-item">
                    <span className="icon">👥</span>
                    <span>Capacity: {room.capacity}</span>
                  </div>
                  <div className="info-item">
                    <span className="icon">💰</span>
                    <span>R{room.pricePerNight} / night</span>
                  </div>
                </div>

                <button 
                  className="book-btn"
                  disabled={!room.isAvailable}
                  onClick={() => window.location.href = `/bookings?roomId=${room.id}`}
                >
                  {room.isAvailable ? 'Book Now' : 'Not Available'}
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

export default RoomsPage;