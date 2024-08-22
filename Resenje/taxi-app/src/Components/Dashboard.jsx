// Dashboard.js
import React from 'react';
import { useLocation } from 'react-router-dom';
import DashboardAdmin from './DashboardAdmin.jsx';
import RiderDashboard from './DashboardRider.jsx';
import DashboardDriver from './DashboardDriver.jsx';

export default function Dashboard() {
  const location = useLocation();
  const user = location.state?.user; //u state sam smestio iz Login pa znam koji je user trenutni
  const userRole = user.roles; // takodje i ulogu isto (admin, rider, driver)

  return (
    <div>
      {userRole === 0 && <DashboardAdmin user={user} />}
      {userRole === 1 && <RiderDashboard user={user} />}
      {userRole === 2 && <DashboardDriver user={user} />}
    </div>
  );
}
