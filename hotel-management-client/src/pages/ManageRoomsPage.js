import React, { useState, useEffect } from 'react';
import { roomAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './ManageRoomsPage.css';

function ManageRoomsPage() {
  const navigate = useNavigate();
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState({ type: '', text: '' });
  const [showForm, setShowForm] = useState(false);
  const [editingRoom, setEditingRoom] = useState(null);
  const [formData, setFormData] = useState({
    roomNumber: '',
    roomType: 'Single',
    capacity: 1,
    pricePerNight: '',
    description: ''
  });

  useEffect(() => {
    console.log('ManageRoomsPage: Component mounted');
    console.log('Current User:', currentUser);

    if (!currentUser || currentUser.role !== 'Admin') {
      alert('Access denied. Admin only.');
      navigate('/');
      return;
    }
    fetchRooms();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Empty array - run only once on mount

  const fetchRooms = async () => {
    setLoading(true);
    try {
      const response = await roomAPI.getAll();
      setRooms(response.data);
    } catch (error) {
      console.error('Error fetching rooms:', error);
      setMessage({ type: 'error', text: 'Failed to load rooms.' });
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const resetForm = () => {
    setFormData({
      roomNumber: '',
      roomType: 'Single',
      capacity: 1,
      pricePerNight: '',
      description: ''
    });
    setEditingRoom(null);
    setShowForm(false);
    setMessage({ type: '', text: '' });
  };

  const handleAddNew = () => {
    resetForm();
    setShowForm(true);
  };

  const handleEdit = (room) => {
    setFormData({
      roomNumber: room.roomNumber,
      roomType: room.roomType,
      capacity: room.capacity,
      pricePerNight: room.pricePerNight,
      description: room.description || ''
    });
    setEditingRoom(room);
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage({ type: '', text: '' });

    // Validation
    if (parseFloat(formData.pricePerNight) <= 0) {
      setMessage({ type: 'error', text: 'Price must be greater than 0' });
      return;
    }

    if (parseInt(formData.capacity) < 1 || parseInt(formData.capacity) > 10) {
      setMessage({ type: 'error', text: 'Capacity must be between 1 and 10' });
      return;
    }

    try {
      if (editingRoom) {
        // Update existing room
        await roomAPI.update(editingRoom.id, formData);
        setMessage({ type: 'success', text: 'Room updated successfully!' });
      } else {
        // Create new room
        await roomAPI.create(formData);
        setMessage({ type: 'success', text: 'Room created successfully!' });
      }

      fetchRooms();
      resetForm();
    } catch (error) {
      if (error.response && error.response.status === 409) {
        setMessage({ type: 'error', text: 'Room number already exists!' });
      } else {
        setMessage({ 
          type: 'error', 
          text: error.response?.data?.message || 'Operation failed. Please try again.' 
        });
      }
    }
  };

  const handleDelete = async (room) => {
    if (!window.confirm(`Are you sure you want to delete Room ${room.roomNumber}?`)) {
      return;
    }

    setMessage({ type: '', text: '' });

    try {
      await roomAPI.delete(room.id);
      setMessage({ type: 'success', text: 'Room deleted successfully!' });
      fetchRooms();
    } catch (error) {
      if (error.response && error.response.status === 400) {
        setMessage({ 
          type: 'error', 
          text: 'Cannot delete room with active bookings.' 
        });
      } else {
        setMessage({ type: 'error', text: 'Failed to delete room.' });
      }
    }
  };

  if (loading) {
    return (
      <div className="manage-rooms-page">
        <div className="loading">Loading rooms...</div>
      </div>
    );
  }

  return (
    <div className="manage-rooms-page">
      <div className="manage-header">
        <h1>🏨 Manage Rooms</h1>
        <p>Add, edit, and delete hotel rooms</p>
      </div>

      <div className="manage-container">
        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        <div className="top-actions">
          <button className="add-new-btn" onClick={handleAddNew}>
            ➕ Add New Room
          </button>
        </div>

        {showForm && (
          <div className="room-form-card">
            <h2>{editingRoom ? '✏️ Edit Room' : '➕ Add New Room'}</h2>
            
            <form onSubmit={handleSubmit} className="room-form">
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="roomNumber">Room Number *</label>
                  <input
                    type="text"
                    id="roomNumber"
                    name="roomNumber"
                    value={formData.roomNumber}
                    onChange={handleChange}
                    placeholder="e.g., 301"
                    maxLength="10"
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="roomType">Room Type *</label>
                  <select
                    id="roomType"
                    name="roomType"
                    value={formData.roomType}
                    onChange={handleChange}
                    required
                  >
                    <option value="Single">Single</option>
                    <option value="Double">Double</option>
                    <option value="Suite">Suite</option>
                    <option value="Deluxe">Deluxe</option>
                    <option value="Presidential">Presidential</option>
                  </select>
                </div>
              </div>

              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="capacity">Capacity *</label>
                  <input
                    type="number"
                    id="capacity"
                    name="capacity"
                    value={formData.capacity}
                    onChange={handleChange}
                    min="1"
                    max="10"
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="pricePerNight">Price per Night (R) *</label>
                  <input
                    type="number"
                    id="pricePerNight"
                    name="pricePerNight"
                    value={formData.pricePerNight}
                    onChange={handleChange}
                    min="0.01"
                    step="0.01"
                    placeholder="e.g., 99.99"
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="description">Description</label>
                <textarea
                  id="description"
                  name="description"
                  value={formData.description}
                  onChange={handleChange}
                  placeholder="Optional room description..."
                  maxLength="500"
                  rows="3"
                />
              </div>

              <div className="form-actions">
                <button type="button" className="cancel-btn" onClick={resetForm}>
                  Cancel
                </button>
                <button type="submit" className="submit-btn">
                  {editingRoom ? 'Update Room' : 'Create Room'}
                </button>
              </div>
            </form>
          </div>
        )}

        <div className="rooms-table-container">
          <h2>All Rooms ({rooms.length})</h2>
          
          {rooms.length === 0 ? (
            <div className="no-rooms">
              <p>No rooms found. Add your first room!</p>
            </div>
          ) : (
            <div className="rooms-table">
              <table>
                <thead>
                  <tr>
                    <th>Room #</th>
                    <th>Type</th>
                    <th>Capacity</th>
                    <th>Price/Night</th>
                    <th>Status</th>
                    <th>Description</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {rooms.map((room) => (
                    <tr key={room.id}>
                      <td><strong>{room.roomNumber}</strong></td>
                      <td>{room.roomType}</td>
                      <td>👥 {room.capacity}</td>
                      <td>R{room.pricePerNight.toFixed(2)}</td>
                      <td>
                        <span className={`status-badge ${room.isAvailable ? 'available' : 'unavailable'}`}>
                          {room.isAvailable ? '✓ Available' : '✗ Booked'}
                        </span>
                      </td>
                      <td className="description-cell">
                        {room.description || 'No description'}
                      </td>
                      <td>
                        <div className="action-buttons">
                          <button 
                            className="edit-btn-small"
                            onClick={() => handleEdit(room)}
                            title="Edit Room"
                          >
                            ✏️
                          </button>
                          <button 
                            className="delete-btn-small"
                            onClick={() => handleDelete(room)}
                            title="Delete Room"
                          >
                            🗑️
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default ManageRoomsPage;