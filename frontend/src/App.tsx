import React, { useState, useEffect } from 'react';
import './App.css';
import './components/styles.css';
import Login from './components/Auth/Login';
import Register from './components/Auth/Register';
import CurrencyConverter from './components/CurrencyConverter/CurrencyConverter';
import ForgotPassword from './components/ForgotPassword';
import CurrencyRates from './components/CurrencyRates';
import Balance from './components/Balance/Balance';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [showLogin, setShowLogin] = useState(true);
  const [showForgotPassword, setShowForgotPassword] = useState(false);
  const [showCurrencyRates, setShowCurrencyRates] = useState(false);
  const [showBalance, setShowBalance] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      setIsAuthenticated(true);
    }
  }, []);

  const handleLoginSuccess = () => {
    setIsAuthenticated(true);
  };

  const handleRegisterSuccess = () => {
    setShowLogin(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setIsAuthenticated(false);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>Santander</h1>
        {isAuthenticated && (
          <div className="header-buttons">
            <button 
              onClick={() => setShowBalance(!showBalance)}
              className={showBalance ? 'active-button' : ''}
            >
              Balance
            </button>
            <button 
              onClick={() => setShowCurrencyRates(!showCurrencyRates)}
              className={showCurrencyRates ? 'active-button' : ''}
            >
              Currency Rates
            </button>
            <button onClick={handleLogout} className="logout-button">
              Logout
            </button>
          </div>
        )}
      </header>
      <main>
        {!isAuthenticated ? (
          <div className="auth-container">
            {showLogin ? (
              <>
                <Login onLoginSuccess={handleLoginSuccess} />
                <button onClick={() => setShowLogin(false)}>
                 Register
                </button>
                <button 
                  className="forgot-password-link"
                  onClick={() => setShowForgotPassword(true)}
                >
                  Forgot Password?
                </button>
              </>
            ) : (
              <>
                <Register onRegisterSuccess={handleRegisterSuccess} />
                <button onClick={() => setShowLogin(true)}>
                  Back to Login
                </button>
              </>
            )}
          </div>
        ) : (
          <>
            <CurrencyConverter />
            {showBalance && <Balance />}
            {showCurrencyRates && <CurrencyRates />}
          </>
        )}

        {showForgotPassword && (
          <ForgotPassword onClose={() => setShowForgotPassword(false)} />
        )}
      </main>
    </div>
  );
}

export default App; 