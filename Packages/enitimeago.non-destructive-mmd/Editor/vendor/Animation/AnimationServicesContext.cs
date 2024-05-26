/*
 * This file is originally from Modular Avatar
 *
 * MIT License
 *
 * Copyright (c) 2022 bd_
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using nadena.dev.ndmf;

namespace enitimeago.NonDestructiveMMD.vendor
{
    /// <summary>
    /// This extension context amortizes a number of animation-related processing steps - notably,
    /// collecting the set of all animation clips from the animators, and committing changes to them
    /// in a deferred manner.
    ///
    /// Restrictions: While this context is active, any changes to clips must be done by editing them via
    /// ClipHolders in the AnimationDatabase. Any newly added clips must be registered in the AnimationDatabase,
    /// and any new references to clips require setting appropriate ClipCommitActions.
    ///
    /// New references to objects created in clips must use paths obtained from the
    /// ObjectRenameTracker.GetObjectIdentifier method.
    /// </summary>
    internal sealed class AnimationServicesContext : IExtensionContext
    {
        private AnimationDatabase _animationDatabase;
        private PathMappings _pathMappings;

        public void OnActivate(BuildContext context)
        {
            _animationDatabase = new AnimationDatabase();
            _animationDatabase.OnActivate(context);

            _pathMappings = new PathMappings();
            _pathMappings.OnActivate(context, _animationDatabase);
        }

        public void OnDeactivate(BuildContext context)
        {
            _pathMappings.OnDeactivate(context);
            _animationDatabase.Commit();

            _pathMappings = null;
            _animationDatabase = null;
        }

        public AnimationDatabase AnimationDatabase
        {
            get
            {
                if (_animationDatabase == null)
                {
                    throw new InvalidOperationException(
                        "AnimationDatabase is not available outside of the AnimationServicesContext");
                }

                return _animationDatabase;
            }
        }

        public PathMappings PathMappings
        {
            get
            {
                if (_pathMappings == null)
                {
                    throw new InvalidOperationException(
                        "ObjectRenameTracker is not available outside of the AnimationServicesContext");
                }

                return _pathMappings;
            }
        }
    }
}
