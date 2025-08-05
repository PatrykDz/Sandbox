import React from 'react';
import { Link } from 'react-router-dom';
import { Brain, Heart, Shield, Award } from 'lucide-react';

const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gray-900 text-white">
      <div className="container mx-auto px-4 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Brand */}
          <div className="col-span-1 md:col-span-2">
            <div className="flex items-center space-x-2 mb-4">
              <div className="w-8 h-8 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center">
                <Brain className="w-5 h-5 text-white" />
              </div>
              <span className="text-xl font-bold">SenteMind</span>
            </div>
            <p className="text-gray-300 mb-6 max-w-md">
              Transform your mental wellness with scientifically-backed tools designed to improve cognitive function, reduce stress, and enhance overall wellbeing.
            </p>
            <div className="flex space-x-6">
              <div className="flex items-center space-x-2 text-sm text-gray-400">
                <Heart className="w-4 h-4" />
                <span>Science-Based</span>
              </div>
              <div className="flex items-center space-x-2 text-sm text-gray-400">
                <Shield className="w-4 h-4" />
                <span>Privacy First</span>
              </div>
              <div className="flex items-center space-x-2 text-sm text-gray-400">
                <Award className="w-4 h-4" />
                <span>Proven Results</span>
              </div>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h3 className="font-semibold mb-4">Tools</h3>
            <ul className="space-y-2">
              <li>
                <Link to="/tools/dual-n-back" className="text-gray-300 hover:text-white transition-colors">
                  Dual N-Back Training
                </Link>
              </li>
              <li>
                <Link to="/tools/mindfulness" className="text-gray-300 hover:text-white transition-colors">
                  Mindfulness Assistant
                </Link>
              </li>
              <li>
                <Link to="/tools/journaling" className="text-gray-300 hover:text-white transition-colors">
                  Smart Journaling
                </Link>
              </li>
              <li>
                <Link to="/tools/breathing" className="text-gray-300 hover:text-white transition-colors">
                  Breath Pacer
                </Link>
              </li>
              <li>
                <Link to="/tools/mood-tracker" className="text-gray-300 hover:text-white transition-colors">
                  Mood Tracker
                </Link>
              </li>
              <li>
                <Link to="/tools/sound-therapy" className="text-gray-300 hover:text-white transition-colors">
                  Sound Therapy
                </Link>
              </li>
            </ul>
          </div>

          {/* Company */}
          <div>
            <h3 className="font-semibold mb-4">Company</h3>
            <ul className="space-y-2">
              <li>
                <Link to="/premium" className="text-gray-300 hover:text-white transition-colors">
                  Premium Features
                </Link>
              </li>
              <li>
                <button className="text-gray-300 hover:text-white transition-colors">
                  About Us
                </button>
              </li>
              <li>
                <button className="text-gray-300 hover:text-white transition-colors">
                  Research
                </button>
              </li>
              <li>
                <button className="text-gray-300 hover:text-white transition-colors">
                  Support
                </button>
              </li>
              <li>
                <button className="text-gray-300 hover:text-white transition-colors">
                  Privacy Policy
                </button>
              </li>
              <li>
                <button className="text-gray-300 hover:text-white transition-colors">
                  Terms of Service
                </button>
              </li>
            </ul>
          </div>
        </div>

        <div className="border-t border-gray-800 mt-8 pt-8 flex flex-col md:flex-row items-center justify-between">
          <p className="text-gray-400 text-sm">
            © {currentYear} SenteMind. All rights reserved.
          </p>
          <p className="text-gray-400 text-sm mt-2 md:mt-0">
            Designed for mental wellness • Built with care
          </p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;