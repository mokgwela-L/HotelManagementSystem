import React from 'react';
import { Navigate } from 'react-router-dom';

function ProtectedRoute({ children, allowedRoles }) {
  // Safely parse user from localStorage
  const currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');

  // If not logged in
  if (!currentUser) {
    console.warn('Access denied: user not logged in');
    return <Navigate to="/login" replace />;
  }

  // If role restriction exists and user does not match
  if (allowedRoles && !allowedRoles.includes(currentUser.role)) {
    console.warn(`Access denied: only for ${allowedRoles.join(' or ')}`);
    return <Navigate to="/" replace />;
  }

  // Otherwise, allow access
  return children;
}

export default ProtectedRoute;
