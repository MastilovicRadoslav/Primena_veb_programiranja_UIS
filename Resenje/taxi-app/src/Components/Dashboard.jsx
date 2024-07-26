import React, { useState } from 'react';
import { useLocation } from 'react-router-dom';
import '../Styles/DashboardPage.css'; 
import DashboardAdmin from './DashboardAdmin.jsx';
import RiderDashboard from './DashboardRider.jsx';
import DashboardDriver from './DashboardDriver.jsx';

export default function Dashboard() {
  const [userName, setUserName] = useState('radoslav.rs');
  const location = useLocation();
  const user = location.state?.user;
  console.log("This is user from main dashboard");
  const userRole = user["roles"];
  const token = localStorage.getItem('token');
  return (
   <div>
     {userRole === 0 && <DashboardAdmin user={user}/>}
     {userRole === 1 && <RiderDashboard user={user}/>}
     {userRole === 2 && <DashboardDriver user={user}/>}
   </div>
  );
}
