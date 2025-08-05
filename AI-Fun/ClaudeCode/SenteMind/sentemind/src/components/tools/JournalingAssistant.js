import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { PenTool, Save, Calendar, TrendingUp, Lightbulb, Heart, Star, Trash2 } from 'lucide-react';

const JournalingAssistant = () => {
  const [currentEntry, setCurrentEntry] = useState('');
  const [entries, setEntries] = useState([]);
  const [selectedPrompt, setSelectedPrompt] = useState(null);
  const [showPrompts, setShowPrompts] = useState(true);
  const [mood, setMood] = useState(3);
  const [tags, setTags] = useState([]);
  const [journalStats, setJournalStats] = useState({
    totalEntries: 0,
    currentStreak: 0,
    averageMood: 3,
    topInsights: []
  });
  const [insights, setInsights] = useState([]);

  const journalPrompts = [
    {
      category: 'Gratitude',
      icon: Heart,
      color: 'from-pink-500 to-rose-500',
      prompts: [
        "What three things am I most grateful for today?",
        "Who in my life am I thankful for and why?",
        "What small moment today brought me joy?",
        "What challenge helped me grow recently?",
        "What aspect of my health am I grateful for?"
      ]
    },
    {
      category: 'Reflection',
      icon: Lightbulb,
      color: 'from-yellow-500 to-orange-500',
      prompts: [
        "What did I learn about myself today?",
        "How did I handle stress or challenges today?",
        "What would I do differently if I could repeat today?",
        "What patterns do I notice in my thoughts and behaviors?",
        "How have I grown in the past month?"
      ]
    },
    {
      category: 'Goals & Dreams',
      icon: Star,
      color: 'from-purple-500 to-indigo-500',
      prompts: [
        "What progress did I make toward my goals today?",
        "What do I want to achieve in the next month?",
        "What's one small step I can take tomorrow toward my dreams?",
        "What obstacles are preventing me from reaching my goals?",
        "How do I want to feel at the end of this year?"
      ]
    },
    {
      category: 'Mindfulness',
      icon: TrendingUp,
      color: 'from-green-500 to-teal-500',
      prompts: [
        "How am I feeling in this exact moment?",
        "What emotions came up for me today?",
        "What did I notice about my thoughts today?",
        "Where did I feel most present and engaged today?",
        "What sensations do I notice in my body right now?"
      ]
    }
  ];

  const moodEmojis = ['😢', '😔', '😐', '😊', '😄'];
  const moodLabels = ['Very Sad', 'Sad', 'Neutral', 'Happy', 'Very Happy'];

  const availableTags = [
    'work', 'relationships', 'health', 'creativity', 'learning', 'travel',
    'family', 'friends', 'goals', 'challenges', 'gratitude', 'mindfulness'
  ];

  const saveEntry = () => {
    if (!currentEntry.trim()) return;

    const newEntry = {
      id: Date.now(),
      content: currentEntry,
      mood: mood,
      tags: tags,
      date: new Date().toISOString(),
      prompt: selectedPrompt
    };

    const updatedEntries = [newEntry, ...entries];
    setEntries(updatedEntries);
    
    // Update stats
    setJournalStats(prev => ({
      totalEntries: prev.totalEntries + 1,
      currentStreak: prev.currentStreak + 1,
      averageMood: (prev.averageMood * prev.totalEntries + mood) / (prev.totalEntries + 1),
      topInsights: prev.topInsights
    }));

    // Generate insights
    generateInsights(updatedEntries);

    // Reset form
    setCurrentEntry('');
    setSelectedPrompt(null);
    setMood(3);
    setTags([]);
    setShowPrompts(true);
  };

  const deleteEntry = (entryId) => {
    const updatedEntries = entries.filter(entry => entry.id !== entryId);
    setEntries(updatedEntries);
    
    setJournalStats(prev => ({
      ...prev,
      totalEntries: Math.max(0, prev.totalEntries - 1)
    }));
  };

  const selectPrompt = (prompt) => {
    setSelectedPrompt(prompt);
    setShowPrompts(false);
  };

  const toggleTag = (tag) => {
    setTags(prev => 
      prev.includes(tag) 
        ? prev.filter(t => t !== tag)
        : [...prev, tag]
    );
  };

  const generateInsights = (entriesData) => {
    const newInsights = [];
    
    if (entriesData.length >= 3) {
      const recentMoods = entriesData.slice(0, 7).map(e => e.mood);
      const avgRecentMood = recentMoods.reduce((a, b) => a + b, 0) / recentMoods.length;
      
      if (avgRecentMood >= 4) {
        newInsights.push({
          type: 'positive',
          text: "You've been in great spirits lately! Your mood has been consistently positive.",
          icon: '🌟'
        });
      } else if (avgRecentMood <= 2) {
        newInsights.push({
          type: 'concern',
          text: "Your mood has been lower recently. Consider what might be affecting you and perhaps talk to someone you trust.",
          icon: '💙'
        });
      }
    }

    if (entriesData.length >= 5) {
      const tagFrequency = {};
      entriesData.forEach(entry => {
        entry.tags.forEach(tag => {
          tagFrequency[tag] = (tagFrequency[tag] || 0) + 1;
        });
      });
      
      const topTag = Object.keys(tagFrequency).reduce((a, b) => 
        tagFrequency[a] > tagFrequency[b] ? a : b, ''
      );
      
      if (topTag) {
        newInsights.push({
          type: 'pattern',
          text: `"${topTag}" appears frequently in your entries. This topic seems important to you right now.`,
          icon: '🔍'
        });
      }
    }

    if (journalStats.currentStreak >= 7) {
      newInsights.push({
        type: 'achievement',
        text: `Amazing! You've maintained a ${journalStats.currentStreak}-day journaling streak. This consistency is building powerful self-awareness habits.`,
        icon: '🔥'
      });
    }

    setInsights(newInsights);
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
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
          <PenTool className="w-12 h-12 inline mr-4 text-blue-600" />
          Smart Journaling Assistant
        </h1>
        <p className="tool-subtitle">
          Develop deeper self-awareness and emotional intelligence through guided journaling with AI-powered insights and personalized prompts.
        </p>
      </div>

      {/* Journal Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-number">{journalStats.totalEntries}</div>
          <div className="stat-label">Total Entries</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{journalStats.currentStreak}</div>
          <div className="stat-label">Day Streak</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{journalStats.averageMood.toFixed(1)}</div>
          <div className="stat-label">Avg Mood</div>
        </div>
        <div className="stat-card">
          <div className="stat-number">{insights.length}</div>
          <div className="stat-label">New Insights</div>
        </div>
      </div>

      <div className="interactive-area">
        {/* Insights Section */}
        {insights.length > 0 && (
          <div className="mb-6">
            <h3 className="text-lg font-bold text-gray-800 mb-3 flex items-center">
              <Lightbulb className="w-5 h-5 mr-2 text-yellow-500" />
              Personal Insights
            </h3>
            <div className="space-y-2">
              {insights.map((insight, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: index * 0.1 }}
                  className={`p-3 rounded-lg ${
                    insight.type === 'positive' ? 'bg-green-50 border-l-4 border-green-400' :
                    insight.type === 'concern' ? 'bg-blue-50 border-l-4 border-blue-400' :
                    insight.type === 'pattern' ? 'bg-purple-50 border-l-4 border-purple-400' :
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

        {/* Writing Section */}
        <div className="mb-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-bold text-gray-800">Today's Entry</h3>
            <button
              onClick={() => setShowPrompts(!showPrompts)}
              className="btn btn-secondary"
            >
              {showPrompts ? 'Hide' : 'Show'} Prompts
            </button>
          </div>

          {/* Journal Prompts */}
          <AnimatePresence>
            {showPrompts && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                className="mb-6"
              >
                <h4 className="font-semibold text-gray-700 mb-3">Choose a prompt to get started:</h4>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {journalPrompts.map((category) => (
                    <div key={category.category} className="space-y-2">
                      <div className={`flex items-center space-x-2 p-2 bg-gradient-to-r ${category.color} rounded-lg text-white`}>
                        <category.icon className="w-4 h-4" />
                        <span className="font-medium text-sm">{category.category}</span>
                      </div>
                      {category.prompts.map((prompt, index) => (
                        <button
                          key={index}
                          onClick={() => selectPrompt(prompt)}
                          className="w-full text-left p-2 text-sm text-gray-600 hover:bg-gray-50 rounded border border-gray-200 transition-colors"
                        >
                          {prompt}
                        </button>
                      ))}
                    </div>
                  ))}
                </div>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Selected Prompt */}
          {selectedPrompt && (
            <div className="mb-4 p-3 bg-blue-50 border-l-4 border-blue-400 rounded">
              <p className="text-blue-800 font-medium">{selectedPrompt}</p>
            </div>
          )}

          {/* Text Area */}
          <textarea
            value={currentEntry}
            onChange={(e) => setCurrentEntry(e.target.value)}
            placeholder={selectedPrompt ? "Reflect on the prompt above..." : "What's on your mind today? How are you feeling?"}
            className="journal-textarea"
          />

          {/* Mood and Tags */}
          <div className="flex flex-col md:flex-row gap-4 mt-4">
            {/* Mood Selector */}
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                How are you feeling today?
              </label>
              <div className="flex items-center space-x-2">
                {moodEmojis.map((emoji, index) => (
                  <button
                    key={index}
                    onClick={() => setMood(index + 1)}
                    className={`w-10 h-10 rounded-full text-lg transition-transform ${
                      mood === index + 1 ? 'scale-125 bg-blue-100' : 'hover:scale-110'
                    }`}
                    title={moodLabels[index]}
                  >
                    {emoji}
                  </button>
                ))}
              </div>
              <p className="text-xs text-gray-500 mt-1">{moodLabels[mood - 1]}</p>
            </div>

            {/* Tags */}
            <div className="flex-1">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tags (optional)
              </label>
              <div className="flex flex-wrap gap-2">
                {availableTags.map(tag => (
                  <button
                    key={tag}
                    onClick={() => toggleTag(tag)}
                    className={`px-3 py-1 text-xs rounded-full transition-colors ${
                      tags.includes(tag)
                        ? 'bg-primary-500 text-white'
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                    }`}
                  >
                    {tag}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Save Button */}
          <div className="mt-4">
            <button
              onClick={saveEntry}
              disabled={!currentEntry.trim()}
              className="btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <Save className="w-4 h-4 mr-2" />
              Save Entry
            </button>
          </div>
        </div>

        {/* Previous Entries */}
        {entries.length > 0 && (
          <div>
            <h3 className="text-lg font-bold text-gray-800 mb-4 flex items-center">
              <Calendar className="w-5 h-5 mr-2" />
              Previous Entries
            </h3>
            <div className="space-y-4 max-h-96 overflow-y-auto">
              {entries.map((entry) => (
                <motion.div
                  key={entry.id}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="journal-entry"
                >
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center space-x-3">
                      <div className="journal-date">{formatDate(entry.date)}</div>
                      <span className="text-lg">{moodEmojis[entry.mood - 1]}</span>
                    </div>
                    <button
                      onClick={() => deleteEntry(entry.id)}
                      className="text-red-500 hover:text-red-700 p-1"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                  
                  {entry.prompt && (
                    <div className="text-xs text-blue-600 mb-2 italic">
                      Prompt: {entry.prompt}
                    </div>
                  )}
                  
                  <div className="journal-content mb-2">{entry.content}</div>
                  
                  {entry.tags.length > 0 && (
                    <div className="flex flex-wrap gap-1">
                      {entry.tags.map(tag => (
                        <span
                          key={tag}
                          className="px-2 py-1 text-xs bg-gray-100 text-gray-600 rounded"
                        >
                          {tag}
                        </span>
                      ))}
                    </div>
                  )}
                </motion.div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Benefits Section */}
      <div className="mt-8 bg-gradient-to-br from-purple-50 to-indigo-50 rounded-lg p-6">
        <h3 className="text-xl font-bold text-gray-800 mb-4">
          The Science of Journaling
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 text-sm text-gray-700">
          <div>
            <h4 className="font-semibold mb-2">Mental Health Benefits:</h4>
            <ul className="space-y-1">
              <li>• Reduces anxiety and depression symptoms</li>
              <li>• Improves emotional regulation</li>
              <li>• Enhances self-awareness and reflection</li>
              <li>• Processes traumatic experiences safely</li>
              <li>• Increases gratitude and positive thinking</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">Cognitive Benefits:</h4>
            <ul className="space-y-1">
              <li>• Improves working memory</li>
              <li>• Enhances problem-solving abilities</li>
              <li>• Increases creativity and insight</li>
              <li>• Better goal setting and achievement</li>
              <li>• Improved communication skills</li>
            </ul>
          </div>
        </div>
        <div className="mt-4 p-4 bg-purple-100 rounded-lg">
          <p className="text-sm text-purple-800 italic">
            <strong>Research shows:</strong> Writing about emotional experiences for just 15-20 minutes a day can significantly improve both mental and physical health. The key is consistency and honest self-reflection.
          </p>
        </div>
      </div>
    </div>
  );
};

export default JournalingAssistant;