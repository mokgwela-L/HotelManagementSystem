import React from 'react';
import { Link } from 'react-router-dom';
import './HomePage.css';

function HomePage() {
  const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');

  return (
    <div className="homepage">
      {/* Hero Section */}
      <section className="hero">
        <div className="overlay"></div>
        <div className="hero-content">
          <h1>Welcome to <span>Ardenveil Hotel</span></h1>
          <p>
            {currentUser
              ? `Hello, ${currentUser.firstName}! Ready to make your next stay unforgettable?`
              : 'Relax. Refresh. Reconnect — your perfect stay awaits.'}
          </p>

          {!currentUser ? (
            <div className="hero-buttons">
              <Link to="/login" className="btn primary">Login</Link>
              <Link to="/register" className="btn secondary">Register</Link>
            </div>
          ) : (
            <div className="hero-buttons">
              <Link to="/rooms" className="btn primary">View Rooms</Link>
              <Link to="/bookings" className="btn secondary">Book Now</Link>
              {currentUser.role === 'Admin' && (
                <Link to="/dashboard" className="btn admin">Go to Dashboard</Link>
              )}
            </div>
          )}
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <h2>Why Choose <span>Ardenveil Hotel</span>?</h2>
        <div className="feature-grid">
          <div className="feature-card">
            <h3>🏨 Luxury Suites</h3>
            <p>Experience world-class comfort and breathtaking views in every room.</p>
          </div>
          <div className="feature-card">
            <h3>💳 Easy Booking</h3>
            <p>Book your stay seamlessly — anytime, anywhere, from any device.</p>
          </div>
          <div className="feature-card">
            <h3>👩‍💼 Friendly Staff</h3>
            <p>Our 24/7 team is always here to make your stay smooth and memorable.</p>
          </div>
          <div className="feature-card">
            <h3>🍽️ Premium Dining</h3>
            <p>Delight your taste buds with world-class cuisine and fine dining options.</p>
          </div>
        </div>
      </section>
    </div>
  );
}

export default HomePage;
