import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import HomePage from './pages/HomePage';
import RegisterPage from './pages/RegisterPage';
import LoginPage from './pages/LoginPage';
import RoomsPage from './pages/RoomsPage';
import BookingsPage from './pages/BookingsPage';
import DashboardPage from './pages/DashboardPage';
import MyBookingsPage from './pages/MyBookingsPage';
import ProfilePage from './pages/ProfilePage';
import ManageRoomsPage from './pages/ManageRoomsPage';
import RatingsPage from './pages/RatingsPage';
import FeedbackPage from './pages/FeedbackPage';
import ProtectedRoute from './Components/ProtectedRoute';
import './App.css';

function App() {
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  const handleLogout = () => {
    localStorage.removeItem('currentUser');
    window.location.href = '/';
  };

  return (
    <Router>
      <div className="App">
        <nav className="navbar">
          <div className="nav-container">
            <Link to="/" className="nav-logo">
              🏨 Hotel Management
            </Link>
            <ul className="nav-menu">
              <li className="nav-item">
                <Link to="/" className="nav-link">Home</Link>
              </li>
              
              {!currentUser && (
                <>
                  <li className="nav-item">
                    <Link to="/register" className="nav-link">Register</Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/login" className="nav-link">Login</Link>
                  </li>
                </>
              )}
              
              {currentUser && (
                <>
                  <li className="nav-item">
                    <Link to="/rooms" className="nav-link">Rooms</Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/bookings" className="nav-link">New Booking</Link>
                  </li>
                  <li className="nav-item">
                    <Link to="/my-bookings" className="nav-link">My Bookings</Link>
                  </li>
                  
                  {/* Guest Only */}
                  {currentUser.role === 'Guest' && (
                    <li className="nav-item">
                      <Link to="/ratings" className="nav-link">Rate Us</Link>
                    </li>
                  )}
                  
                  {/* Receptionist Only */}
                  {currentUser.role === 'Receptionist' && (
                    <li className="nav-item">
                      <Link to="/feedback" className="nav-link">Guest Feedback</Link>
                    </li>
                  )}
                  
                  {/* Admin Only */}
                  {currentUser.role === 'Admin' && (
                    <>
                      <li className="nav-item">
                        <Link to="/dashboard" className="nav-link">Dashboard</Link>
                      </li>
                      <li className="nav-item">
                        <Link to="/manage-rooms" className="nav-link">Manage Rooms</Link>
                      </li>
                    </>
                  )}
                  
                  <li className="nav-item">
                    <Link to="/profile" className="nav-link">Profile</Link>
                  </li>
                  
                  <li className="nav-item">
                    <span className="nav-link user-welcome">
                      👤 {currentUser.firstName} ({currentUser.role})
                    </span>
                  </li>
                  <li className="nav-item">
                    <button onClick={handleLogout} className="logout-btn">
                      Logout
                    </button>
                  </li>
                </>
              )}
            </ul>
          </div>
        </nav>

        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/login" element={<LoginPage />} />
          
          <Route 
            path="/rooms" 
            element={
              <ProtectedRoute>
                <RoomsPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/bookings" 
            element={
              <ProtectedRoute>
                <BookingsPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/my-bookings" 
            element={
              <ProtectedRoute>
                <MyBookingsPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/profile" 
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            } 
          />
          
          {/* Guest Only */}
          <Route 
            path="/ratings" 
            element={
              <ProtectedRoute allowedRoles={['Guest']}>
                <RatingsPage />
              </ProtectedRoute>
            } 
          />
          
          {/* Receptionist Only */}
          <Route 
            path="/feedback" 
            element={
              <ProtectedRoute allowedRoles={['Receptionist']}>
                <FeedbackPage />
              </ProtectedRoute>
            } 
          />
          
          {/* Admin Only */}
          <Route 
            path="/dashboard" 
            element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <DashboardPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/manage-rooms" 
            element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <ManageRoomsPage />
              </ProtectedRoute>
            } 
          />
        </Routes>
      </div>
    </Router>
  );
}

export default App;