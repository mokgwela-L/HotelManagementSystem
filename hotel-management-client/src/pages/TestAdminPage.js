import React from 'react';

function TestAdminPage() {
  const currentUser = JSON.parse(localStorage.getItem('currentUser'));

  return (
    <div style={{ padding: '50px', textAlign: 'center' }}>
      <h1>Admin Test Page</h1>
      <p>If you see this, routing works!</p>
      <pre>{JSON.stringify(currentUser, null, 2)}</pre>
    </div>
  );
}

export default TestAdminPage;