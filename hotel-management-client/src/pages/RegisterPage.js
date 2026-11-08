import React, { useState } from 'react';
import { guestAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './RegisterPage.css';

function RegisterPage() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    address: '',
    password: '',
    confirmPassword: ''
  });

  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const validateForm = () => {
    if (!formData.firstName.trim()) {
      setMessage({ type: 'error', text: 'First name is required' });
      return false;
    }
    if (!formData.lastName.trim()) {
      setMessage({ type: 'error', text: 'Last name is required' });
      return false;
    }
    if (!formData.email.trim() || !formData.email.includes('@')) {
      setMessage({ type: 'error', text: 'Valid email is required' });
      return false;
    }
    if (!formData.phoneNumber.trim()) {
      setMessage({ type: 'error', text: 'Phone number is required' });
      return false;
    }
    if (!formData.password || formData.password.length < 6) {
      setMessage({ type: 'error', text: 'Password must be at least 6 characters' });
      return false;
    }
    if (formData.password !== formData.confirmPassword) {
      setMessage({ type: 'error', text: 'Passwords do not match' });
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
    console.log('Sending registration data:', formData); // Debug log
    
    const response = await guestAPI.register(formData);
    
    console.log('Registration response:', response); // Debug log
    
    setMessage({ 
      type: 'success', 
      text: `Registration successful! Redirecting to login...` 
    });
    
    // Redirect to login after 2 seconds
    setTimeout(() => {
      navigate('/login');
    }, 2000);
  } catch (error) {
    console.error('Registration error:', error); // Debug log
    console.error('Error response:', error.response); // Debug log
    
    if (error.response) {
      // Server responded with error
      if (error.response.status === 409) {
        setMessage({ 
          type: 'error', 
          text: 'A guest with this email already exists!' 
        });
      } else if (error.response.status === 400) {
        setMessage({ 
          type: 'error', 
          text: error.response.data.message || 'Invalid data. Please check all fields.' 
        });
      } else {
        setMessage({ 
          type: 'error', 
          text: `Server error: ${error.response.status}` 
        });
      }
    } else if (error.request) {
      // Request made but no response
      setMessage({ 
        type: 'error', 
        text: 'Cannot connect to server. Please check if API is running.' 
      });
    } else {
      // Something else went wrong
      setMessage({ 
        type: 'error', 
        text: `Error: ${error.message}` 
      });
    }
  } finally {
    setLoading(false);
  }
};

  return (
    <div className="register-page">
      <div className="register-container">
        <h1>Guest Registration</h1>
        <p className="subtitle">Create your account to start booking</p>

        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        <form onSubmit={handleSubmit} className="register-form">
          <div className="form-group">
            <label htmlFor="firstName">First Name *</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              placeholder="Enter first name"
              maxLength="50"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="lastName">Last Name *</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              placeholder="Enter last name"
              maxLength="50"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email *</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="your.email@example.com"
              maxLength="100"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="phoneNumber">Phone Number *</label>
            <input
              type="tel"
              id="phoneNumber"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleChange}
              placeholder="+1234567890"
              maxLength="20"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="address">Address</label>
            <textarea
              id="address"
              name="address"
              value={formData.address}
              onChange={handleChange}
              placeholder="Enter your address (optional)"
              maxLength="200"
              rows="3"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password *</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="At least 6 characters"
              minLength="6"
              maxLength="100"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password *</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Re-enter your password"
              minLength="6"
              maxLength="100"
              required
            />
          </div>

          <button 
            type="submit" 
            className="submit-btn"
            disabled={loading}
          >
            {loading ? 'Registering...' : 'Register'}
          </button>

          <p className="login-link">
            Already have an account? <a href="/login">Login here</a>
          </p>
        </form>
      </div>
    </div>
  );
}

export default RegisterPage;