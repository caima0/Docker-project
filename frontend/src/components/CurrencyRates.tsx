import React, { useState, useEffect } from 'react';
import { currencyService, CurrencyRate } from '../services/api';

const CurrencyRates: React.FC = () => {
  const [rates, setRates] = useState<CurrencyRate[]>([]);
  const [filteredRates, setFilteredRates] = useState<CurrencyRate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [editingRate, setEditingRate] = useState<CurrencyRate | null>(null);
  const [updating, setUpdating] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      setIsAuthenticated(true);
      const payload = JSON.parse(atob(token.split('.')[1]));
      setIsAdmin(payload.role === 'Admin');
    } else {
      setIsAuthenticated(false);
    }
  }, []);

  const fetchRates = async () => {
    try {
      const data = await currencyService.getRates();
      setRates(data);
      setFilteredRates(data);
      setError('');
    } catch (err: any) {
      setError('Failed to fetch currency rates');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isAuthenticated) {
      fetchRates();
    }
  }, [isAuthenticated]);

  useEffect(() => {
    const filtered = rates.filter(rate => 
      rate.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      rate.currency.toLowerCase().includes(searchTerm.toLowerCase())
    );
    setFilteredRates(filtered);
  }, [searchTerm, rates]);

  const handleEdit = (rate: CurrencyRate) => {
    if (!isAdmin) {
      setError('Only administrators can edit rates');
      return;
    }
    setEditingRate(rate);
  };

  const handleDelete = async (code: string) => {
    if (!isAdmin) {
      setError('Only administrators can delete rates');
      return;
    }
    if (window.confirm('Are you sure you want to delete this rate?')) {
      try {
        await currencyService.deleteRate(code);
        setRates(rates.filter(rate => rate.code !== code));
        setError('');
      } catch (err: any) {
        setError('Failed to delete rate');
      }
    }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingRate || !isAdmin) return;

    try {
      const updatedRate = await currencyService.updateRate(editingRate.code, editingRate);
      setRates(rates.map(rate => 
        rate.code === editingRate.code ? updatedRate : rate
      ));
      setEditingRate(null);
      setError('');
    } catch (err: any) {
      setError('Failed to update rate');
    }
  };

  const handleUpdateFromNBP = async () => {
    if (!isAdmin) {
      setError('Only administrators can update rates from NBP');
      return;
    }
    try {
      setUpdating(true);
      setError('');
      await currencyService.updateRatesFromNBP();
      await fetchRates();
    } catch (err: any) {
      setError('Failed to update rates from NBP');
    } finally {
      setUpdating(false);
    }
  };

  if (!isAuthenticated) {
    return null;
  }

  if (loading) return <div>Loading...</div>;

  return (
    <div className="currency-rates">
      <div className="rates-header">
        <h2>Currency Rates</h2>
        <div className="rates-controls">
          <input
            type="text"
            placeholder="Search by code or currency..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          {isAdmin && (
            <button 
              onClick={handleUpdateFromNBP} 
              disabled={updating}
              className="update-button"
            >
              {updating ? 'Updating...' : 'Update Rates from NBP'}
            </button>
          )}
        </div>
      </div>
      {error && <div className="error-message">{error}</div>}
      
      <table>
        <thead>
          <tr>
            <th>Code</th>
            <th>Currency</th>
            <th>Bid</th>
            <th>Ask</th>
            {isAdmin && <th>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {filteredRates.map(rate => (
            <tr key={rate.code}>
              <td>{rate.code}</td>
              <td>{rate.currency}</td>
              <td>{rate.bid.toFixed(4)}</td>
              <td>{rate.ask.toFixed(4)}</td>
              {isAdmin && (
                <td>
                  <button onClick={() => handleEdit(rate)}>Edit</button>
                  <button onClick={() => handleDelete(rate.code)}>Delete</button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>

      {editingRate && (
        <div className="edit-modal">
          <div className="modal-content">
            <h3>Edit Rate</h3>
            <form onSubmit={handleUpdate}>
              <div className="form-group">
                <label>Code:</label>
                <input
                  type="text"
                  value={editingRate.code}
                  onChange={e => setEditingRate({...editingRate, code: e.target.value})}
                  required
                />
              </div>
              <div className="form-group">
                <label>Currency:</label>
                <input
                  type="text"
                  value={editingRate.currency}
                  onChange={e => setEditingRate({...editingRate, currency: e.target.value})}
                  required
                />
              </div>
              <div className="form-group">
                <label>Bid:</label>
                <input
                  type="number"
                  step="0.0001"
                  value={editingRate.bid}
                  onChange={e => setEditingRate({...editingRate, bid: parseFloat(e.target.value)})}
                  required
                />
              </div>
              <div className="form-group">
                <label>Ask:</label>
                <input
                  type="number"
                  step="0.0001"
                  value={editingRate.ask}
                  onChange={e => setEditingRate({...editingRate, ask: parseFloat(e.target.value)})}
                  required
                />
              </div>
              <div className="button-group">
                <button type="submit">Save</button>
                <button type="button" onClick={() => setEditingRate(null)}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default CurrencyRates; 