import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { TrendingUp, Calendar, BarChart3, PieChart, Target, Plus } from 'lucide-react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart as RechartsPieChart, Cell } from 'recharts';

const MoodTracker = () => {
  const [currentMood, setCurrentMood] = useState(3);
  const [currentNote, setCurrentNote] = useState('');
  const [selectedFactors, setSelectedFactors] = useState([]);
  const [moodEntries, setMoodEntries] = useState([]);
  const [viewMode, setViewMode] = useState('week'); // week, month, year
  const [showAddEntry, setShowAddEntry] = useState(true);
  const [insights, setInsights] = useState([]);

  const moods = [
    { value: 1, emoji: '😢', label: 'Very Sad', color: '#ef4444' },
    { value: 2, emoji: '😔', label: 'Sad', color: '#f97316' },
    { value: 3, emoji: '😐', label: 'Neutral', color: '#eab308' },
    { value: 4, emoji: '😊', label: 'Happy', color: '#22c55e' },
    { value: 5, emoji: '😄', label: 'Very Happy', color: '#10b981' }
  ];

  const moodFactors = [
    'sleep', 'exercise', 'work', 'relationships', 'weather', 'health',
    'social', 'stress', 'nutrition', 'meditation', 'hobbies', 'travel'
  ];

  const [stats, setStats] = useState({
    averageMood: 3,
    totalEntries: 0,
    streak: 0,
    bestDay: null,
    worstDay: null,
    topFactors: []
  });

  const addMoodEntry = () => {
    if (!currentMood) return;

    const newEntry = {
      id: Date.now(),
      mood: currentMood,
      note: currentNote,
      factors: selectedFactors,
      date: new Date().toISOString(),
      timestamp: Date.now()
    };

    const updatedEntries = [newEntry, ...moodEntries];
    setMoodEntries(updatedEntries);

    // Update stats
    updateStats(updatedEntries);
    generateInsights(updatedEntries);

    // Reset form
    setCurrentNote('');
    setSelectedFactors([]);
    setShowAddEntry(false);
  };

  const updateStats = (entries) => {
    if (entries.length === 0) return;

    const averageMood = entries.reduce((sum, entry) => sum + entry.mood, 0) / entries.length;
    const bestEntry = entries.reduce((best, entry) => entry.mood > best.mood ? entry : best);
    const worstEntry = entries.reduce((worst, entry) => entry.mood < worst.mood ? entry : worst);

    // Calculate factor frequency
    const factorCount = {};
    entries.forEach(entry => {
      entry.factors.forEach(factor => {
        factorCount[factor] = (factorCount[factor] || 0) + 1;
      });
    });

    const topFactors = Object.entries(factorCount)
      .sort(([,a], [,b]) => b - a)
      .slice(0, 5)
      .map(([factor, count]) => ({ factor, count }));

    // Calculate streak (consecutive days with entries)
    const today = new Date();
    let streak = 0;
    for (let i = 0; i < 30; i++) {
      const checkDate = new Date(today);
      checkDate.setDate(today.getDate() - i);
      const hasEntry = entries.some(entry => {
        const entryDate = new Date(entry.date);
        return entryDate.toDateString() === checkDate.toDateString();
      });
      if (hasEntry) {
        streak++;
      } else {
        break;
      }
    }

    setStats({
      averageMood: averageMood,
      totalEntries: entries.length,
      streak: streak,
      bestDay: bestEntry,
      worstDay: worstEntry,
      topFactors: topFactors
    });
  };

  const generateInsights = (entries) => {
    const newInsights = [];

    if (entries.length >= 7) {
      const last7Days = entries.slice(0, 7);
      const avgLast7 = last7Days.reduce((sum, entry) => sum + entry.mood, 0) / 7;
      
      if (avgLast7 >= 4) {
        newInsights.push({
          type: 'positive',
          text: "Your mood has been consistently positive this week! Keep up whatever you're doing.",
          icon: '🌟'
        });
      } else if (avgLast7 <= 2) {
        newInsights.push({
          type: 'concern',
          text: "Your mood has been lower this week. Consider what might be affecting you and reach out for support if needed.",
          icon: '💙'
        });
      }

      // Check for patterns
      const weekdayMoods = {};
      last7Days.forEach(entry => {
        const day = new Date(entry.date).getDay();
        weekdayMoods[day] = weekdayMoods[day] || [];
        weekdayMoods[day].push(entry.mood);
      });

      const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      let bestDay = -1;
      let bestDayAvg = 0;
      
      Object.entries(weekdayMoods).forEach(([day, moods]) => {
        const avg = moods.reduce((a, b) => a + b, 0) / moods.length;
        if (avg > bestDayAvg) {
          bestDayAvg = avg;
          bestDay = parseInt(day);
        }
      });

      if (bestDay >= 0 && bestDayAvg >= 4) {
        newInsights.push({
          type: 'pattern',
          text: `${dayNames[bestDay]}s seem to be your best days! Your mood averages ${bestDayAvg.toFixed(1)} on this day.`,
          icon: '📊'
        });
      }
    }

    if (stats.topFactors.length > 0) {
      const topFactor = stats.topFactors[0];
      const factorEntries = entries.filter(entry => entry.factors.includes(topFactor.factor));
      const factorAvgMood = factorEntries.reduce((sum, entry) => sum + entry.mood, 0) / factorEntries.length;
      
      if (factorAvgMood >= 4) {
        newInsights.push({
          type: 'insight',
          text: `"${topFactor.factor}" appears frequently in your positive mood entries. This seems to boost your wellbeing!`,
          icon: '🔍'
        });
      } else if (factorAvgMood <= 2) {
        newInsights.push({
          type: 'insight',
          text: `"${topFactor.factor}" often coincides with lower moods. Consider how to better manage this factor.`,
          icon: '⚠️'
        });
      }
    }

    if (stats.streak >= 7) {
      newInsights.push({
        type: 'achievement',
        text: `Impressive! You've tracked your mood for ${stats.streak} consecutive days. This consistency is building valuable self-awareness.`,
        icon: '🔥'
      });
    }

    setInsights(newInsights);
  };

  const getChartData = () => {
    const days = viewMode === 'week' ? 7 : viewMode === 'month' ? 30 : 365;
    const data = [];
    const today = new Date();

    for (let i = days - 1; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(today.getDate() - i);
      
      const dayEntries = moodEntries.filter(entry => {
        const entryDate = new Date(entry.date);
        return entryDate.toDateString() === date.toDateString();
      });

      const avgMood = dayEntries.length > 0 
        ? dayEntries.reduce((sum, entry) => sum + entry.mood, 0) / dayEntries.length
        : null;

      data.push({
        date: date.toLocaleDateString('en-US', { 
          month: 'short', 
          day: 'numeric',
          ...(viewMode === 'year' && { year: '2-digit' })
        }),
        mood: avgMood,
        entries: dayEntries.length
      });
    }

    return data;
  };

  const getMoodDistribution = () => {
    const distribution = moods.map(mood => ({
      name: mood.label,
      value: moodEntries.filter(entry => entry.mood === mood.value).length,
      color: mood.color
    }));
    return distribution.filter(item => item.value > 0);
  };

  const toggleFactor = (factor) => {
    setSelectedFactors(prev =>
      prev.includes(factor)
        ? prev.filter(f => f !== factor)
        : [...prev, factor]
    );
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="tool-container">
      <div className="tool-header">
        <h1 className="tool-title">
          <TrendingUp className="w-12 h-12 inline mr-4 text-purple-600" />
          Mood Tracker
        </h1>
        <p className="tool-subtitle">
          Track your emotional patterns, identify triggers, and gain insights into your mental wellbeing through data-driven mood analysis.
        </p>
      </div>

      {/* Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{stats.totalEntries}</div>
          <div className="stat-label">Total Entries</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{stats.averageMood.toFixed(1)}</div>
          <div className="stat-label">Average Mood</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{stats.streak}</div>
          <div className="stat-label">Day Streak</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{insights.length}</div>
          <div className="stat-label">New Insights</div>
        </div>
      </div>

      <div className="interactive-area">
        {/* Insights */}
        {insights.length > 0 && (
          <div className="mb-6">
            <h3 className="text-lg font-bold text-gray-800 mb-3">Personal Insights</h3>
            <div className="space-y-2">
              {insights.map((insight, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: index * 0.1 }}
                  className={`p-3 rounded-lg ${
                    insight.type === 'positive' ? 'bg-green-50 border-l-4 border-green-400' :
                    insight.type === 'concern' ? 'bg-red-50 border-l-4 border-red-400' :
                    insight.type === 'pattern' ? 'bg-blue-50 border-l-4 border-blue-400' :
                    insight.type === 'insight' ? 'bg-purple-50 border-l-4 border-purple-400' :
                    'bg-orange-50 border-l-4 border-orange-400'
                  }`}
                >
                  <div className="flex items-start space-x-2">
                    <span className="text-lg">{insight.icon}</span>
                    <p className="text-sm text-gray-700">{insight.text}</p>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        )}

        {/* Add Entry */}
        {showAddEntry && (
          <div className="mb-6 bg-gray-50 rounded-lg p-4">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-bold text-gray-800">How are you feeling right now?</h3>
              <button
                onClick={() => setShowAddEntry(false)}
                className="text-gray-500 hover:text-gray-700"
              >
                ✕
              </button>
            </div>

            {/* Mood Selection */}
            <div className="mood-grid mb-4">
              {moods.map((mood) => (
                <motion.div
                  key={mood.value}
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  className={`mood-option ${currentMood === mood.value ? 'selected' : ''}`}
                  onClick={() => setCurrentMood(mood.value)}
                >
                  <div className="mood-emoji">{mood.emoji}</div>
                  <div className="mood-label">{mood.label}</div>
                </motion.div>
              ))}
            </div>

            {/* Factors */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                What factors might be influencing your mood? (optional)
              </label>
              <div className="flex flex-wrap gap-2">
                {moodFactors.map(factor => (
                  <button
                    key={factor}
                    onClick={() => toggleFactor(factor)}
                    className={`px-3 py-1 text-sm rounded-full transition-colors capitalize ${
                      selectedFactors.includes(factor)
                        ? 'bg-purple-500 text-white'
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                    }`}
                  >
                    {factor}
                  </button>
                ))}
              </div>
            </div>

            {/* Note */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Add a note (optional)
              </label>
              <textarea
                value={currentNote}
                onChange={(e) => setCurrentNote(e.target.value)}
                placeholder="What's happening? How do you feel? Any specific thoughts?"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
                rows="3"
              />
            </div>

            <button
              onClick={addMoodEntry}
              disabled={!currentMood}
              className="btn btn-primary disabled:opacity-50"
            >
              <Plus className="w-4 h-4 mr-2" />
              Track Mood
            </button>
          </div>
        )}

        {!showAddEntry && (
          <div className="mb-6 text-center">
            <button
              onClick={() => setShowAddEntry(true)}
              className="btn btn-primary"
            >
              <Plus className="w-4 h-4 mr-2" />
              Add Mood Entry
            </button>
          </div>
        )}

        {/* Charts */}
        {moodEntries.length > 0 && (
          <div className="space-y-6">
            {/* View Mode Selector */}
            <div className="flex justify-center space-x-2">
              {['week', 'month', 'year'].map(mode => (
                <button
                  key={mode}
                  onClick={() => setViewMode(mode)}
                  className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors capitalize ${
                    viewMode === mode
                      ? 'bg-purple-500 text-white'
                      : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                  }`}
                >
                  {mode}
                </button>
              ))}
            </div>

            {/* Mood Trend Chart */}
            <div className="bg-white p-4 rounded-lg">
              <h4 className="text-lg font-semibold mb-4 flex items-center">
                <BarChart3 className="w-5 h-5 mr-2" />
                Mood Trend ({viewMode})
              </h4>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={getChartData()}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis domain={[1, 5]} />
                  <Tooltip 
                    formatter={(value) => [value ? value.toFixed(1) : 'No data', 'Mood']}
                    labelFormatter={(label) => `Date: ${label}`}
                  />
                  <Line 
                    type="monotone" 
                    dataKey="mood" 
                    stroke="#8b5cf6" 
                    strokeWidth={2}
                    dot={{ fill: '#8b5cf6', strokeWidth: 2, r: 4 }}
                    connectNulls={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>

            {/* Mood Distribution */}
            <div className="bg-white p-4 rounded-lg">
              <h4 className="text-lg font-semibold mb-4 flex items-center">
                <PieChart className="w-5 h-5 mr-2" />
                Mood Distribution
              </h4>
              <div className="flex items-center justify-center">
                <ResponsiveContainer width="100%" height={300}>
                  <RechartsPieChart>
                    <RechartsPieChart
                      data={getMoodDistribution()}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={120}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {getMoodDistribution().map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </RechartsPieChart>
                    <Tooltip />
                  </RechartsPieChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Top Factors */}
            {stats.topFactors.length > 0 && (
              <div className="bg-white p-4 rounded-lg">
                <h4 className="text-lg font-semibold mb-4 flex items-center">
                  <Target className="w-5 h-5 mr-2" />
                  Most Common Factors
                </h4>
                <div className="space-y-2">
                  {stats.topFactors.map((factor, index) => (
                    <div key={factor.factor} className="flex items-center justify-between">
                      <span className="capitalize text-gray-700">{factor.factor}</span>
                      <div className="flex items-center space-x-2">
                        <div className="w-24 bg-gray-200 rounded-full h-2">
                          <div
                            className="bg-purple-500 h-2 rounded-full"
                            style={{ width: `${(factor.count / moodEntries.length) * 100}%` }}
                          />
                        </div>
                        <span className="text-sm text-gray-500">{factor.count}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Recent Entries */}
            <div className="bg-white p-4 rounded-lg">
              <h4 className="text-lg font-semibold mb-4 flex items-center">
                <Calendar className="w-5 h-5 mr-2" />
                Recent Entries
              </h4>
              <div className="space-y-3 max-h-64 overflow-y-auto">
                {moodEntries.slice(0, 10).map((entry) => (
                  <div key={entry.id} className="flex items-start space-x-3 p-3 bg-gray-50 rounded">
                    <div className="text-2xl">{moods[entry.mood - 1].emoji}</div>
                    <div className="flex-1">
                      <div className="flex items-center justify-between mb-1">
                        <span className="font-medium text-gray-800">
                          {moods[entry.mood - 1].label}
                        </span>
                        <span className="text-sm text-gray-500">
                          {formatDate(entry.date)}
                        </span>
                      </div>
                      {entry.note && (
                        <p className="text-sm text-gray-600 mb-2">{entry.note}</p>
                      )}
                      {entry.factors.length > 0 && (
                        <div className="flex flex-wrap gap-1">
                          {entry.factors.map(factor => (
                            <span
                              key={factor}
                              className="px-2 py-1 text-xs bg-purple-100 text-purple-700 rounded capitalize"
                            >
                              {factor}
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {moodEntries.length === 0 && !showAddEntry && (
          <div className="text-center py-12">
            <TrendingUp className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <h3 className="text-xl text-gray-600 mb-2">Start Tracking Your Mood</h3>
            <p className="text-gray-500 mb-4">
              Begin your journey of self-awareness by logging your first mood entry.
            </p>
            <button
              onClick={() => setShowAddEntry(true)}
              className="btn btn-primary"
            >
              <Plus className="w-4 h-4 mr-2" />
              Add First Entry
            </button>
          </div>
        )}
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-purple-50 to-pink-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4">
          Why Track Your Mood?
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Emotional Intelligence:</h4>
            <ul className="space-y-1">
              <li>• Identify emotional patterns and triggers</li>
              <li>• Improve self-awareness and regulation</li>
              <li>• Better understand your emotional responses</li>
              <li>• Develop healthier coping strategies</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Mental Health Benefits:</h4>
            <ul className="space-y-1">
              <li>• Early detection of mood changes</li>
              <li>• Track effectiveness of treatments</li>
              <li>• Identify lifestyle factors affecting wellbeing</li>
              <li>• Better communication with healthcare providers</li>
            </ul>
          </div>
        </div>
        <p className="text-sm text-gray-600 mt-4 italic">
          Research shows that regular mood tracking can significantly improve emotional regulation and help prevent depressive episodes.
        </p>
      </div>
    </div>
  );
};

export default MoodTracker;