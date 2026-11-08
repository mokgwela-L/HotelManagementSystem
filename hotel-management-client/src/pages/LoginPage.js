import React, { useState } from 'react';
import { guestAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import './LoginPage.css';

function LoginPage() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });

  const [message, setMessage] = useState({ type: '', text: '' });
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
  e.preventDefault();
  setMessage({ type: '', text: '' });

  if (!formData.email || !formData.password) {
    setMessage({ type: 'error', text: 'Please enter both email and password' });
    return;
  }

  setLoading(true);

  try {
    console.log('Sending login data:', { email: formData.email }); // Debug log (don't log password!)
    
    const response = await guestAPI.login(formData);
    
    console.log('Login response:', response.data); // Debug log
    
    if (response.data.success) {
      // Store user info in localStorage
      localStorage.setItem('currentUser', JSON.stringify(response.data.guest));
      
      setMessage({ 
        type: 'success', 
        text: `Welcome back, ${response.data.guest.firstName}!` 
      });
      
      // Redirect to bookings page
      setTimeout(() => {
        navigate('/bookings');
      }, 1500);
    } else {
      setMessage({ 
        type: 'error', 
        text: response.data.message || 'Invalid email or password' 
      });
    }
  } catch (error) {
    console.error('Login error:', error); // Debug log
    console.error('Error response:', error.response); // Debug log
    
    if (error.response) {
      setMessage({ 
        type: 'error', 
        text: `Server error: ${error.response.status}. Please try again.` 
      });
    } else if (error.request) {
      setMessage({ 
        type: 'error', 
        text: 'Cannot connect to server. Please check if API is running.' 
      });
    } else {
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
    <div className="login-page">
      <div className="login-container">
        <h1>Welcome Back!</h1>
        <p className="subtitle">Login to your account</p>

        {message.text && (
          <div className={`message ${message.type}`}>
            {message.text}
          </div>
        )}

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="your.email@example.com"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="Enter your password"
              required
            />
          </div>

          <button 
            type="submit" 
            className="submit-btn"
            disabled={loading}
          >
            {loading ? 'Logging in...' : 'Login'}
          </button>

          <p className="register-link">
            Don't have an account? <a href="/register">Register here</a>
          </p>
        </form>
      </div>
    </div>
  );
}

export default LoginPage;